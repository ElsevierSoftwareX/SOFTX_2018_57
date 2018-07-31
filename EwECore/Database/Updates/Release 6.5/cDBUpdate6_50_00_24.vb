' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

Option Strict On
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.24:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Updated shape functions</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_24
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500024!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Updated shape functions"
        End Get
    End Property

    Private s_tables As String() = New String() {"EcosimShapeTime", "EcosimShapeEggProd", "EcosimShapeMediation"}
    Private s_fields As String() = New String() {"YZero", "YBase", "YEnd", "Steep"}

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True

        For Each strTable As String In s_tables
            Try
                bSuccess = bSuccess And Me.UpdateShapeFunctions(db, strTable)
            Catch ex As Exception
                bSuccess = False
            End Try
        Next

        Return bSuccess

    End Function

    Private Function UpdateShapeFunctions(db As cEwEDatabase, strTableName As String) As Boolean

        Dim bSuccess As Boolean = db.Execute("ALTER TABLE " & strTableName & " ADD COLUMN FunctionParams MEMO")
        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter(strTableName)
        Dim dt As DataTable = writer.GetDataTable()
        Dim bIsChanged As Boolean = False

        For Each drow As DataRow In dt.Rows

            Dim functiontype As Long = 0
            Dim parms(3) As Single
            Dim XMin As Single = 0
            Dim XMax As Single = 0
            Dim strZScale As String = CStr(drow("zScale"))
            Dim bIsMediation As Boolean = dt.Columns.Contains("XAxisMin")

            bIsChanged = False

            If (Not Convert.IsDBNull(drow("FunctionType"))) Then

                functiontype = CLng(drow("FunctionType"))

                Dim Y0 As Single = CSng(drow("YZero"))
                Dim YB As Single = CSng(drow("YBase"))
                Dim YE As Single = CSng(drow("YEnd"))
                Dim St As Single = CSng(drow("Steep"))

                If (bIsMediation) Then
                    XMin = CSng(If(Convert.IsDBNull(drow("XAxisMin")), 0, drow("XAxisMin")))
                    XMax = CSng(If(Convert.IsDBNull(drow("XAxisMax")), 0, drow("XAxisMax")))
                End If

                Select Case functiontype
                    Case eShapeFunctionType.Betapdf

                        Dim pts As String() = strZScale.Split(" "c)
                        Dim max As Single = 0
                        For Each pt As String In pts
                            max = Math.Max(max, cStringUtils.ConvertToSingle(pt))
                        Next

                        parms(0) = Y0 ' A
                        parms(1) = YE ' B
                        parms(2) = max ' Scalar
                        bIsChanged = True

                    Case eShapeFunctionType.Exponential,
                         eShapeFunctionType.Hyperbolic,
                         eShapeFunctionType.Sigmoid_Legacy
                        ' These three functions need reworking, the type and its parameters are not usable anymore.
                        ' For backwards compatibility just keep the point data and forget the original primitive.
                        functiontype = 0
                        bIsChanged = True

                    Case eShapeFunctionType.Linear
                        parms(0) = Y0 ' Start
                        parms(1) = YE ' End
                        bIsChanged = True

                    Case eShapeFunctionType.Normal
                        'Update the Normal distribution parameters 
                        'by adding the a value for the Mean and Datawidth fields 
                        'Update 6_50_00_27 will update to the new format where the Xmin, XMax and datawidth are calculated from the mean and sd
                        Dim normaldist As cNormalShapeFunction = New cNormalShapeFunction()
                        If Me.ConvertNormalDistribution(normaldist, St, Y0, YE, YB, XMin, XMax) Then

                            ReDim parms(normaldist.nParameters - 1)
                            For ipr As Integer = 1 To normaldist.nParameters
                                parms(ipr - 1) = normaldist.ParamValue(ipr)
                            Next

                            Dim PointsArray() As Single = normaldist.Shape(cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS)
                            Dim sbZScale As New Text.StringBuilder()
                            For ipt As Integer = 1 To cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS
                                If (ipt > 1) Then sbZScale.Append(" ")
                                sbZScale.Append(cStringUtils.FormatSingle(PointsArray(ipt)))
                            Next
                            strZScale = sbZScale.ToString()
                            sbZScale.Clear()
                            bIsChanged = True
                        End If

                    Case eShapeFunctionType.LeftShoulder, eShapeFunctionType.RightShoulder
                        parms(0) = Y0 ' Left point
                        parms(1) = YE ' Right point
                        parms(2) = YB ' Width
                        bIsChanged = True

                    Case eShapeFunctionType.Trapezoid
                        parms(0) = Y0 ' Left bottom
                        parms(1) = YE ' Left top
                        parms(2) = YB ' Right top
                        parms(3) = St ' Right bottom
                        bIsChanged = True

                End Select

            End If

            If (bIsChanged) Then
                drow.BeginEdit()
                drow("FunctionType") = functiontype
                drow("FunctionParams") = cStringUtils.ParamArrayToString(parms)
                drow("zScale") = strZScale
                If (bIsMediation) Then
                    drow("XAxisMin") = XMin
                    drow("XAxisMax") = XMax
                End If
                drow.EndEdit()
            End If

        Next

        writer.Commit()
        db.ReleaseWriter(writer, True)

        For i As Integer = 0 To 3
            bSuccess = bSuccess And db.Execute("ALTER TABLE " & strTableName & " DROP COLUMN " & s_fields(i))
        Next

        Return bSuccess

    End Function

    Private Function ConvertNormalDistribution(ByVal normaldist As cNormalShapeFunction,
                                               ByVal Mean As Single, ByVal SDLeft As Single, ByVal SDRight As Single, ByVal DataWidth As Single, _
                                               ByRef Xmin As Single, ByRef XMax As Single) As Boolean

        Mean = Xmin + (XMax - Xmin) / 2
        DataWidth = SDLeft * 5 + SDRight * 5

        normaldist.Mean = Mean
        normaldist.DataWidth = DataWidth
        normaldist.SDLeft = SDLeft
        normaldist.SDRight = SDRight
        normaldist.NormalMax = 1

        Return True

    End Function

End Class
