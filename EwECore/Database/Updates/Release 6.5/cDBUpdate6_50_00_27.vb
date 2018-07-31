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

#Region " Imports "

Option Strict On
Imports EwEUtils.Database
Imports EwEUtils.Utilities
Imports EwEUtils.SystemUtilities

#End Region ' Imports

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.50.0.27:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Updated Ecosim environmental drivers</description></item>
''' <item><description>Updated existing Normal distribution shape functions</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_50_00_27
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.500027!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Updated Ecosim environmental drivers, discontinued obsolete fields"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Me.UpdateResponseFunctions(db)
        Return Me.ConvertEcosimEnvDrivers(db) And Me.Cleanup(db)

    End Function

    Private Function ConvertEcosimEnvDrivers(ByVal db As cEwEDatabase) As Boolean

        Dim nGroups As Integer = CInt(db.GetValue("SELECT COUNT(*) FROM EcopathGroup", 0))
        Dim astrGroupNames(nGroups) As String
        Dim aiGroupDBID(nGroups) As Integer
        Dim iGroup As Integer = 1
        Dim nScenarios As Integer = CInt(db.GetValue("SELECT COUNT(*) FROM EcosimScenario", 0))
        Dim astrScenarioNames(nScenarios) As String
        Dim aiScenarioDBID(nScenarios) As Integer
        Dim iScenario As Integer = 1
        Dim reader As IDataReader = Nothing
        Dim bSuccess As Boolean = True

        reader = db.GetReader("SELECT * FROM EcopathGroup ORDER BY Sequence ASC")
        While reader.Read()
            aiGroupDBID(iGroup) = CInt(reader("GroupID"))
            astrGroupNames(iGroup) = CStr(reader("GroupName"))
            iGroup += 1
        End While
        db.ReleaseReader(reader)

        reader = db.GetReader("SELECT * FROM EcosimScenario")
        While reader.Read()
            aiScenarioDBID(iScenario) = CInt(reader("ScenarioID"))
            astrScenarioNames(iScenario) = CStr(reader("ScenarioName"))
            iScenario += 1
        End While
        db.ReleaseReader(reader)

        reader = db.GetReader("SELECT * FROM EcoSimScenarioGroup")
        While reader.Read()

            Dim iScenarioID As Integer = CInt(reader("ScenarioID"))
            Dim iGroupID As Integer = CInt(reader("GroupID"))
            Dim iEcopathGroupID As Integer = CInt(reader("EcopathGroupID"))

            iGroup = Array.IndexOf(aiGroupDBID, iEcopathGroupID)
            iScenario = Array.IndexOf(aiScenarioDBID, iScenarioID)

            If (iGroup > 0) Then
                Dim sSO As Single = CSng(db.ReadSafe(reader, "SalOpt", 35.0!))
                Dim sSL As Single = CSng(db.ReadSafe(reader, "SdSalLeft", 1000.0!))
                Dim sSR As Single = CSng(db.ReadSafe(reader, "SdSalRight", 1000.0!))

                If (sSR <> 1000) Or (sSL <> 1000) Then

                    Dim iDriverIDSal As Integer = CInt(db.GetValue("SELECT MAX(SalinityForcingShapeID) FROM EcosimScenario WHERE (ScenarioID=" & iScenarioID & ")"))
                    Dim iResponseID As Integer = 0
                    bSuccess = bSuccess And Me.CreateReponseCurve(db, "Salinity", iGroup, astrGroupNames(iGroup), astrScenarioNames(iScenario), sSO, sSL, sSR, iResponseID) And
                                            Me.AssignResponse(db, iScenarioID, iGroupID, iDriverIDSal, iResponseID)
                End If

            End If

        End While
        db.ReleaseReader(reader)

        Me.m_dtCurves.Clear()

        ' Next create Temperature responses
        reader = db.GetReader("SELECT * FROM EcoSimScenarioGroup")
        While reader.Read()

            Dim iScenarioID As Integer = CInt(reader("ScenarioID"))
            Dim iGroupID As Integer = CInt(reader("GroupID"))
            Dim iEcopathGroupID As Integer = CInt(reader("EcopathGroupID"))

            iGroup = Array.IndexOf(aiGroupDBID, iEcopathGroupID)
            iScenario = Array.IndexOf(aiScenarioDBID, iScenarioID)

            If (iGroup > 0) Then
                Dim sTO As Single = CSng(db.ReadSafe(reader, "TempOpt", 10.0!))
                Dim sTL As Single = CSng(db.ReadSafe(reader, "TempLeft", 1000.0!))
                Dim sTR As Single = CSng(db.ReadSafe(reader, "TempRight", 1000.0!))

                If (sTR <> 1000) Or (sTL <> 1000) Then

                    Dim iDriverTemID As Integer = CInt(db.GetValue("SELECT MAX(TemperatureForcingShapeID) FROM EcosimScenario WHERE (ScenarioID=" & iScenarioID & ")"))
                    Dim iResponseID As Integer = 0
                    bSuccess = bSuccess And Me.CreateReponseCurve(db, "Temp", iGroup, astrGroupNames(iGroup), astrScenarioNames(iScenario), sTO, sTL, sTR, iResponseID) And
                                            Me.AssignResponse(db, iScenarioID, iGroupID, iDriverTemID, iResponseID)
                End If
            End If

        End While

        db.ReleaseReader(reader)

        Return bSuccess

    End Function

    Private Function Cleanup(ByVal db As cEwEDatabase) As Boolean

        Dim bSuccess As Boolean = True
        For Each str As String In New String() {"SalOpt", "SdSalLeft", "SdSalRight", "TempOpt", "TempLeft", "TempRight"}
            bSuccess = bSuccess And db.Execute("ALTER TABLE EcoSimScenarioGroup DROP COLUMN " & str)
        Next
        For Each str As String In New String() {"SalinityForcingShapeID", "TemperatureForcingShapeID"}
            Dim strIndex As String = db.GetIndexName("EcosimScenario", str)
            If (Not String.IsNullOrWhiteSpace(strIndex)) Then
                bSuccess = bSuccess And db.Execute("DROP INDEX " & strIndex & " ON EcosimScenario")
            End If
            bSuccess = bSuccess And db.Execute("ALTER TABLE EcosimScenario DROP COLUMN " & str)
        Next
        Return bSuccess

    End Function

    Private m_dtCurves As New Dictionary(Of String, Integer)

    Private Function Hash(ByVal iGroup As Integer, ByVal sOpt As Single, ByVal sStdLeft As Single, ByVal sStdRight As Single) As String
        Return String.Format("{0}@{1}:{2}:{3}", iGroup, sStdLeft, sOpt, sStdRight)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new response function from a sal or temp mean and optimum definition.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function CreateReponseCurve(ByVal db As cEwEDatabase,
                                       ByVal strResponse As String, ByVal iGroup As Integer, ByVal strGroupName As String, ByVal strScenario As String,
                                       ByVal sOpt As Single, ByVal sStdLeft As Single, ByVal sStdRight As Single, ByRef iShapeID As Integer) As Boolean

        Dim bSuccess As Boolean = True

        Dim strHash As String = Me.Hash(iGroup, sOpt, sStdLeft, sStdRight)
        ' Is this response is already defined?
        If (Me.m_dtCurves.ContainsKey(strHash)) Then
            ' #Yes: return existing response curve
            iShapeID = Me.m_dtCurves(strHash)
            Return True
        End If

        iShapeID = CInt(db.GetValue("SELECT MAX(ShapeID) FROM EcoSimShape", 0)) + 1

        Dim sfn As New cNormalShapeFunction()
        Dim writerID As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcoSimShape")
        Dim writerShape As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcosimShapeMediation")
        Dim drow As DataRow = writerID.NewRow()
        Dim sbZScale As New Text.StringBuilder()

        Try
            drow("ShapeID") = iShapeID
            drow("ShapeType") = EwEUtils.Core.eDataTypes.CapacityMediation
            drow("IsSeasonal") = False
            writerID.AddRow(drow)
            writerID.Commit()

            drow = writerShape.NewRow()
            drow("ShapeID") = iShapeID
            ' Shape name field cannot exceed 50 characters
            drow("Title") = cStringUtils.MaxLength(String.Format("{0} {1} - {2}", strResponse, strGroupName, strScenario), 50)

            sfn.SDLeft = sStdLeft
            sfn.SDRight = sStdRight
            sfn.Mean = sOpt
            sfn.NormalMax = 1
            Dim pts As Single() = sfn.Shape(cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS)
            Dim parms(sfn.nParameters) As Single

            For ipt As Integer = 1 To cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS
                If (ipt > 1) Then sbZScale.Append(" ")
                sbZScale.Append(cStringUtils.FormatSingle(pts(ipt)))
            Next
            drow("zScale") = sbZScale.ToString()

            For ipr As Integer = 1 To sfn.nParameters
                parms(ipr - 1) = sfn.ParamValue(ipr)
            Next
            drow("FunctionType") = EwEUtils.Core.eShapeFunctionType.Normal
            drow("FunctionParams") = cStringUtils.ParamArrayToString(parms, sfn.nParameters)
            drow("IMedBase") = cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS / 3
            drow("XAxisMin") = sfn.Mean - sfn.DataWidth / 2
            drow("XAxisMax") = sfn.Mean + sfn.DataWidth / 2
            writerShape.AddRow(drow)
            writerShape.Commit()

            db.ReleaseWriter(writerShape, True)
            db.ReleaseWriter(writerID)

            Me.m_dtCurves(strHash) = iShapeID

        Catch ex As Exception
            '  Me.LogMessage(String.Format("Error {0} occurred while appending shape {1}, {2}", ex.Message, strShapeName, shapeType.ToString()))
            bSuccess = False
        End Try

        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Assign the newly defined response shape to an existing driver, if any. 
    ''' </summary>
    ''' <param name="db">Plop.</param>
    ''' <param name="iScenarioID">The Ecosim sccenario to assign the response to.</param>
    ''' <param name="iGroupID">The Ecosim group to assign the response to.</param>
    ''' <param name="iDriverID">The existing Ecosim driver forcing function (sal or temp).</param>
    ''' <param name="iResponseID">The newly created Ecosim environmental response shape.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function AssignResponse(ByVal db As cEwEDatabase,
                                    ByVal iScenarioID As Integer, ByVal iGroupID As Integer,
                                    ByVal iDriverID As Integer, ByVal iResponseID As Integer) As Boolean

        If (iDriverID <= 0) Then Return True

        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcosimScenarioCapacityDrivers")
        Dim drow As DataRow = writer.NewRow()

        drow("ScenarioID") = iScenarioID
        drow("GroupID") = iGroupID
        drow("DriverID") = iDriverID
        drow("ResponseID") = iResponseID

        writer.AddRow(drow)
        db.ReleaseWriter(writer)

        Return True

    End Function

    ''' <summary>
    ''' Update Existing EcosimShapeMediation Normal distribution functions records from the old format which scaled to function to a fixed format based on the Mean and SD values.
    ''' </summary>
    ''' <param name="db"></param>
    ''' <remarks></remarks>
    Private Sub UpdateResponseFunctions(ByVal db As cEwEDatabase)

        Dim writer As cEwEDatabase.cEwEDbWriter = db.GetWriter("EcosimShapeMediation")
        Dim dt As DataTable = writer.GetDataTable()
        Dim Mean As Single, SDLeft As Single, SDRight As Single, DataWidth As Single
        Dim XMin As Single, XMax As Single
        Dim PointsArray() As Single

        Dim NormDist As cNormalShapeFunction = New cNormalShapeFunction()
        Dim parms(NormDist.nParameters) As Single
        Dim sbZScale As New Text.StringBuilder()

        Dim rows() As DataRow = dt.Select("FunctionType = " + CLng(EwEUtils.Core.eShapeFunctionType.Normal).ToString)
        For Each row As DataRow In rows

            Debug.Print(row("Title").ToString)

            Me.ReadMediationParameters(row, Mean, SDLeft, SDRight, DataWidth, XMin, XMax)
            'ConvertNormalDistribution(...) will return True if the values were updated
            If Me.ConvertNormalDistribution(Mean, SDLeft, SDRight, DataWidth, XMin, XMax, PointsArray, NormDist) Then
                'This row was updated to the new fixed scale format
                'Save the results
                row.BeginEdit()

                For ipr As Integer = 1 To NormDist.nParameters
                    parms(ipr - 1) = NormDist.ParamValue(ipr)
                Next
                row("FunctionParams") = cStringUtils.ParamArrayToString(parms, NormDist.nParameters)

                sbZScale.Clear()
                For ipt As Integer = 1 To cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS
                    If (ipt > 1) Then sbZScale.Append(" ")
                    sbZScale.Append(cStringUtils.FormatSingle(PointsArray(ipt)))
                Next
                row("zScale") = sbZScale.ToString()

                row("XAxisMin") = XMin
                row("XAxisMax") = XMax

                'for debugging which row has been updated
                'row("Title") = "Edited Row " + ir.ToString
                row.EndEdit()

            End If

        Next
        'DON'T Call AcceptChanges() 
        'Because the code in cEwEDbWrite.Commit() checks the HasChanges Flag
        'If the changes have been accepted then it will not Update the Adapter...
        'dt.AcceptChanges()
        writer.Commit()

        db.ReleaseWriter(writer)

    End Sub


    Private Sub ReadMediationParameters(MediationDataRow As DataRow, ByRef Mean As Single, ByRef SDLeft As Single, ByRef SDRight As Single, ByRef DataWidth As Single, _
                                         ByRef Xmin As Single, ByRef XMax As Single)

        Try
            Dim pars() As Single = cStringUtils.StringToParamArray(CStr(MediationDataRow("FunctionParams")))
            SDLeft = pars(0)
            DataWidth = pars(1)
            SDRight = pars(2)
            Mean = pars(3)

            Xmin = CSng(MediationDataRow("XAxisMin"))
            XMax = CSng(MediationDataRow("XAxisMax"))
        Catch ex As Exception
            ' FunctionParms could be empty, you never know
            ' Keep quiet, plow on, nothing to see here folks
        End Try

    End Sub

    Private Function ConvertNormalDistribution(ByRef Mean As Single, ByRef SDLeft As Single, ByRef SDRight As Single, ByRef DataWidth As Single, _
                                         ByRef Xmin As Single, ByRef XMax As Single, ByRef Points() As Single, normalDistFunction As cNormalShapeFunction) As Boolean

        Dim SD As Single = (Mean - Xmin) / (DataWidth / 2)
        If Math.Round(SD, 3) <> 1 Then

            Debug.Print("----------------")
            Debug.Print("SD " + SD.ToString)

            SDLeft *= SD
            SDRight *= SD

            DataWidth = SDLeft * 5 + SDRight * 5

            Xmin = Mean - DataWidth / 2
            XMax = Mean + DataWidth / 2

            normalDistFunction.Mean = Mean
            normalDistFunction.SDLeft = SDLeft
            normalDistFunction.SDRight = SDRight
            normalDistFunction.DataWidth = DataWidth

            Points = normalDistFunction.Shape(cMediationDataStructures.N_DEFAULT_MEDIATIONPOINTS)
            Return True

        End If

        Return False

    End Function


End Class
