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
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports System.Xml
Imports System.Globalization
Imports EwECore
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls

    ''' ===========================================================================
    ''' <summary>
    ''' Class for maintaining a Trophic Pyramid.
    ''' </summary>
    ''' ===========================================================================
    Public Class cPyramid

#Region " Pyramid calculation constants "

        Private Const c_sMidCutScaleFactor As Single = 23.0!
        Private Const c_sMidCutLogCeiling As Single = 200.0!
        Private Const c_sHighCutoff As Single = 100.0!
        Private Const c_sHighCutoffAngle As Single = 19.0!
        Private Const c_sLowCutoff As Single = 4.0!
        Private Const c_sLowCutoffAngle As Single = 90.0!

#End Region ' Pyramid calculation constants

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Pyramid types, enum values based on Network Analysis pyramid file formats
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Enum ePyramidTypes As Byte
            [Catch] = 0
            Flow = 1
            Biomass = 2
        End Enum

#Region " Private vars "

        ''' <summary>Name of the model that the pyramid relates to.</summary>
        Private m_strModel As String = ""
        ''' <summary>States whether the pyramid data is valid.</summary>
        Private m_bValid As Boolean = False

        ''' <summary>Number of TL read from data file.</summary>
        Private m_iNumTLMax As Integer = 0
        ''' <summary>Number of TL the user wants to see.</summary>
        Private m_iNumTL As Integer = 0
        ''' <summary>Type of pyramid.</summary>
        Private m_pyramidtype As ePyramidTypes = ePyramidTypes.Catch
        ''' <summary>Pyramid value units.</summary>
        Private m_strUnit As String = ""
        ''' <summary>Total biomass.</summary>
        Private m_sTotal As Single = cCore.NULL_VALUE
        ''' <summary>Biomass per trophic level.</summary>
        Private m_asTRB() As Single = Nothing
        ''' <summary>TE per trophic level.</summary>
        Private m_asValue() As Single = Nothing

        ''' <summary>Calculated height of the pyramid.</summary>
        Private m_sHeight As Single = 0
        ''' <summary>Calculated width of the pyramid.</summary>
        Private m_sWidth As Single = 0
        ''' <summary>Values for trophic levels [0, 1].</summary>
        Private m_asValues() As Single

#End Region ' Private vars

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, creates an empty pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New()
            Me.Calculate()
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, creates a new pyramid.
        ''' </summary>
        ''' <param name="strModel">Name of the model to create the pyramid for.</param>
        ''' <param name="pyramidtype"></param>
        ''' <param name="strUnit"></param>
        ''' <param name="iNumTL"></param>
        ''' <param name="sTotalB"></param>
        ''' <param name="asBiomass"></param>
        ''' <param name="asValue"></param>
        ''' <remarks></remarks>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal strModel As String, _
                       ByVal pyramidtype As cPyramid.ePyramidTypes, _
                       ByVal strUnit As String, ByVal iNumTL As Integer, _
                       ByVal sTotalB As Single, ByVal asBiomass() As Single, ByVal asValue() As Single)

            Me.m_strModel = strModel
            Me.m_pyramidtype = pyramidtype
            Me.m_strUnit = strUnit
            Me.m_iNumTLMax = iNumTL
            Me.m_iNumTL = CInt(Math.Ceiling(iNumTL / 2))
            Me.m_sTotal = sTotalB
            Me.m_asTRB = asBiomass
            Me.m_asValue = asValue
            Me.Calculate()

        End Sub

#Region " Public properties "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the name of the model that reflects the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Model() As String
            Get
                Return Me.m_strModel
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get whether the data few to the pyramid is valid.
        ''' </summary>
        ''' <remarks>
        ''' Simple validation is performed when reading pyramid data. For now, a
        ''' pyramid is valid if more than one TL of data was provided, and all
        ''' provided TL were successfully read.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property IsValid() As Boolean
            Get
                Return Me.m_bValid
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the actual number of trophic levels with biomasses.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property FunctionalNumTL() As Integer
            Get
                Dim iTL As Integer = 0
                While (Me.m_asTRB(iTL) > 0) And (iTL < Me.m_iNumTLMax)
                    iTL += 1
                End While
                Return iTL
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Number of TL to display.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Property NumTL() As Integer
            Get
                Return Math.Min(Me.m_iNumTL, Me.m_iNumTLMax)
            End Get
            Set(ByVal iNumTL As Integer)
                Me.m_iNumTL = iNumTL
                Me.Calculate()
            End Set
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the value for a given trophic level.
        ''' </summary>
        ''' <param name="iTL"></param>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Value(ByVal iTL As Integer) As Single
            Get
                If (iTL < 0 Or iTL > Me.m_iNumTL) Then Return 0.0!
                Return Me.m_asValues(iTL)
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Max number of TL in the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property MaxNumTL() As Integer
            Get
                Return Me.m_iNumTLMax
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Unscaled width of the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Width() As Single
            Get
                Return Me.m_sWidth
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Unscaled height of the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Height() As Single
            Get
                Return Me.m_sHeight
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the <see cref="ePyramidTypes">value type</see> fo the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property PyramidType() As ePyramidTypes
            Get
                Return Me.m_pyramidtype
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Get the formatted units of the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property Units() As String
            Get
                Return Me.m_strUnit
            End Get
        End Property

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the sum of biomasses in the pyramid.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property SumB() As Single
            Get
                If (Me.m_sTotal <> cCore.NULL_VALUE) Then Return Me.m_sTotal

                Dim sSumB As Single = 0.0
                For iTL As Integer = 0 To Me.NumTL
                    sSumB += Me.m_asTRB(iTL)
                Next iTL
                Return sSumB

            End Get
        End Property

#End Region ' Public properties

#Region " File I/O "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Export a pyramid to XML.
        ''' </summary>
        ''' <param name="strFilename"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Exported XML will state numbers using decimal points, not allowing for
        ''' thousands separators.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Public Function ToXML(ByVal strFilename As String) As Boolean

            Dim doc As XmlDocument = New XmlDocument()
            Dim nodePyramid As XmlNode = Nothing
            Dim attrib As XmlAttribute = Nothing
            Dim nodeTL As XmlNode = Nothing
            Dim ciEnUSLocale As New CultureInfo("en-US")

            Try

                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", ""))
                nodePyramid = doc.CreateElement("Pyramid")
                doc.AppendChild(nodePyramid)

                attrib = doc.CreateAttribute("Model")
                attrib.Value = Me.m_strModel
                nodePyramid.Attributes.Append(attrib)

                attrib = doc.CreateAttribute("Type")
                attrib.Value = Me.m_pyramidtype.ToString()
                nodePyramid.Attributes.Append(attrib)

                attrib = doc.CreateAttribute("Unit")
                attrib.Value = Me.m_strUnit
                nodePyramid.Attributes.Append(attrib)

                attrib = doc.CreateAttribute("TotalBiomass")
                attrib.Value = Me.m_sTotal.ToString(ciEnUSLocale)
                nodePyramid.Attributes.Append(attrib)

                attrib = doc.CreateAttribute("NumTL")
                attrib.Value = Me.m_iNumTLMax.ToString(ciEnUSLocale)
                nodePyramid.Attributes.Append(attrib)

                For iTL As Integer = 1 To Me.m_iNumTLMax
                    nodeTL = doc.CreateElement("TrophicLevel")

                    attrib = doc.CreateAttribute("Seq")
                    attrib.Value = iTL.ToString(ciEnUSLocale)
                    nodeTL.Attributes.Append(attrib)

                    Select Case Me.PyramidType
                        Case ePyramidTypes.Biomass : attrib = doc.CreateAttribute("Biomass")
                        Case ePyramidTypes.Catch : attrib = doc.CreateAttribute("Catch")
                        Case ePyramidTypes.Flow : attrib = doc.CreateAttribute("Throughput")
                    End Select
                    attrib.Value = Me.m_asTRB(iTL).ToString(ciEnUSLocale)
                    nodeTL.Attributes.Append(attrib)

                    Select Case Me.PyramidType
                        Case ePyramidTypes.Biomass : attrib = doc.CreateAttribute("RelativeBiomass")
                        Case ePyramidTypes.Catch : attrib = doc.CreateAttribute("RelativeCatch")
                        Case ePyramidTypes.Flow : attrib = doc.CreateAttribute("RelativeThroughput")
                    End Select
                    attrib.Value = cStringUtils.FormatSingle(Me.m_asValue(iTL))
                    nodeTL.Attributes.Append(attrib)

                    nodePyramid.AppendChild(nodeTL)
                Next iTL

                doc.Save(strFilename)

            Catch ex As Exception
                Return False
            End Try
            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Import a pyramid from XML.
        ''' </summary>
        ''' <param name="strFilename"></param>
        ''' <param name="bFixedFormatting">Flag stating whether to interpret
        ''' file content using fixed number formatting (True), assuming decimal points
        ''' and not allowing for thousands separators, or using number formatting as
        ''' defined by the language settings in Windows (False).</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Function FromXML(ByVal strFilename As String, _
                                Optional ByVal bFixedFormatting As Boolean = True) As Boolean

            Dim doc As XmlDocument = New XmlDocument()
            Dim iTL As Integer = 0
            Dim sVal1 As Single = 0.0!
            Dim sVal2 As Single = 0.0!

            Try
                Me.Clear()
                doc.Load(strFilename)
            Catch ex As Exception
                Return False
            End Try

            Try

                For Each nodePyramid As XmlNode In doc.ChildNodes
                    If String.Compare(nodePyramid.Name, "pyramid", True) = 0 Then
                        ' Found 'pyramid' node, scam attributes
                        For Each attrib As XmlAttribute In nodePyramid.Attributes
                            Select Case attrib.Name.ToLower()
                                Case "model"
                                    Me.m_strModel = attrib.Value
                                Case "type"
                                    Me.m_pyramidtype = DirectCast([Enum].Parse(GetType(ePyramidTypes), attrib.Value), ePyramidTypes)
                                Case "unit"
                                    Me.m_strUnit = attrib.Value
                                Case "totalbiomass"
                                    If bFixedFormatting Then
                                        Me.m_sTotal = cStringUtils.ConvertToSingle(attrib.Value, 0.0!)
                                    Else
                                        Me.m_sTotal = Convert.ToSingle(attrib.Value)
                                    End If
                                Case "numtl"
                                    If bFixedFormatting Then
                                        Me.m_iNumTLMax = cStringUtils.ConvertToInteger(attrib.Value, 0)
                                    Else
                                        Me.m_iNumTLMax = Convert.ToInt16(attrib.Value)
                                    End If
                                Case Else ' NOP
                            End Select
                        Next

                        ReDim Me.m_asTRB(Me.m_iNumTLMax)
                        ReDim Me.m_asValue(Me.m_iNumTLMax)

                        For Each nodeTL As XmlNode In nodePyramid.ChildNodes
                            If String.Compare(nodeTL.Name, "trophiclevel", True) = 0 Then
                                ' Found TL node
                                iTL = 0 : sVal1 = 0.0! : sVal2 = 0.0!
                                For Each attrib As XmlAttribute In nodeTL.Attributes
                                    Select Case attrib.Name.ToLower()
                                        Case "seq"
                                            If bFixedFormatting Then
                                                iTL = cStringUtils.ConvertToInteger(attrib.Value, 0)
                                            Else
                                                iTL = Convert.ToInt16(attrib.Value, 0)
                                            End If
                                        Case "biomass", "catch", "throughput"
                                            If bFixedFormatting Then
                                                sVal1 = cStringUtils.ConvertToSingle(attrib.Value, 0.0!)
                                            Else
                                                sVal1 = Convert.ToSingle(attrib.Value)
                                            End If
                                        Case "relativebiomass", "relativecatch", "relativethroughput"
                                            If bFixedFormatting Then
                                                sVal2 = cStringUtils.ConvertToSingle(attrib.Value, 0.0!)
                                            Else
                                                sVal2 = Convert.ToSingle(attrib.Value)
                                            End If
                                        Case Else ' NOP
                                    End Select
                                Next
                                Me.m_asTRB(iTL) = sVal1
                                Me.m_asValue(iTL) = sVal2
                            End If
                        Next
                    End If
                Next
            Catch ex As Exception
                Me.Clear()
                Return False
            End Try

            Me.Calculate()

            Return True

        End Function

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Read a pyramid from an EwE5 text file.
        ''' </summary>
        ''' <param name="strFileName">The name of the EwE5 text file to read.</param>
        ''' <param name="bFixedFormatting">Flag stating whether to interpret
        ''' file content using fixed number formatting (True), assuming decimal points
        ''' and not allowing for thousands separators, or using number formatting as
        ''' defined by the language settings in Windows (False).</param>
        ''' <returns>True if successful.</returns>
        ''' -----------------------------------------------------------------------
        Public Function FromEwE5TextFile(ByVal strFileName As String, _
                                         Optional ByVal bFixedFormatting As Boolean = True) As Boolean

            Dim tr As TextReader = Nothing
            Dim strLine As String = ""
            Dim bSucces As Boolean = True

            Me.m_bValid = False

            If (Not File.Exists(strFileName)) Then Return False

            tr = New StreamReader(strFileName, New System.Text.UTF8Encoding())
            Try
                strLine = tr.ReadLine()
                Me.m_pyramidtype = DirectCast(Convert.ToByte(strLine.Substring(0, 1)), ePyramidTypes)
                Me.m_iNumTLMax = Convert.ToInt16(strLine.Substring(1))
                ReDim Me.m_asTRB(Me.m_iNumTLMax)
                ReDim Me.m_asValue(Me.m_iNumTLMax)

                Me.m_strUnit = tr.ReadLine()

                If bFixedFormatting Then
                    Me.m_sTotal = cStringUtils.ConvertToSingle(tr.ReadLine())
                Else
                    Me.m_sTotal = Convert.ToSingle(tr.ReadLine())
                End If

                For iTL As Integer = 0 To Me.m_iNumTLMax - 1
                    strLine = tr.ReadLine()
                    If bFixedFormatting Then
                        Me.m_asTRB(iTL) = cStringUtils.ConvertToSingle(strLine.Substring(0, 12))
                        Me.m_asValue(iTL) = cStringUtils.ConvertToSingle(strLine.Substring(12))
                    Else
                        Me.m_asTRB(iTL) = Convert.ToSingle(strLine.Substring(0, 12))
                        Me.m_asValue(iTL) = Convert.ToSingle(strLine.Substring(12))
                    End If
                Next iTL

                ' Update
                Me.NumTL = Me.FunctionalNumTL
                Me.m_bValid = True

            Catch e As Exception
                bSucces = False
            Finally
                tr.Close()
            End Try

            Return bSucces
        End Function

#End Region ' File IO

#Region " Internals "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Returns the top angle of pyramid for the current number of trophic levels.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public ReadOnly Property TopAngle() As Single
            Get
                Dim dAverageTE As Double = 0.0
                Dim dY As Double = 0.0
                Dim iNumTE As Integer = 1
                Dim sTopAngle As Single = 90.0!
                Dim sTEpowNumTL As Single = 0.0!

                ' JS: Revised the number of trophic levels calculation. Previously, only TLs 1-4 were used. 
                '     Changed this logic to loop up to configurable value NumTL
                dAverageTE = 1.0
                iNumTE = 1
                While (Me.m_asValue(iNumTE) > 0 And iNumTE <= Me.NumTL)
                    dAverageTE *= Me.m_asValue(iNumTE)
                    iNumTE += 1
                End While
                dY = CDbl(1 / iNumTE)

                'if(pDoc->TrEf[3] != 0) 		//tro_levels = 3; level 4
                '{
                '		y=(double)(.333);
                '		AveTrEf = pDoc->TrEf[1]*pDoc->TrEf[2]*pDoc->TrEf[3];
                '}
                'else if(pDoc->TrEf[2] != 0) 	//tro_levels = 2; level 3
                '{
                '		y=(double)(.5);
                '		AveTrEf = pDoc->TrEf[1]*pDoc->TrEf[2];
                '}
                'else 						//tro_levels = 1; level 2
                '{
                '		y=(double)(1.0);
                '		AveTrEf = pDoc->TrEf[1];
                '}
                sTEpowNumTL = CSng(Math.Pow(dAverageTE, dY))

                ' VC used a series of hard constants to 'make it look good'
                If (sTEpowNumTL > cPyramid.c_sHighCutoff) Then
                    sTopAngle = cPyramid.c_sHighCutoffAngle
                ElseIf (sTEpowNumTL > cPyramid.c_sLowCutoff) Then
                    sTopAngle = CSng(cPyramid.c_sMidCutScaleFactor! * Math.Log10(cPyramid.c_sMidCutLogCeiling / sTEpowNumTL))
                Else
                    sTopAngle = cPyramid.c_sLowCutoffAngle
                End If

                Return sTopAngle
            End Get
        End Property

        Private Sub Calculate()

            Me.m_bValid = (Me.NumTL > 0)

            If Not Me.IsValid Then Return

            ' Use surface calcs to position levels
            '     .
            '    /|\ α = half top angle
            '   / | \
            '  /  |  \  h = height
            ' /___|___\
            '       w = half base width
            '
            ' tan(α) = w/h : w = h*tan(α)
            ' where α = half top angle
            '       h = pyramid height
            '       w = pyramid half base width
            '
            ' Half area of pyramid R = Σ{num}/2 = SumB/2
            ' Half area R = w*h/2 = h^2*tan(α)/2 => h = √(2*R/tan(α)) = √(SumB/tan(α))

            Dim sAngleDeg As Single = Me.TopAngle
            Dim sHalfAngleRad As Single = CSng(sAngleDeg * Math.PI / 360.0!)
            Dim sTanHalfAngleRad As Single = CSng(Math.Tan(sHalfAngleRad))
            Dim sSumTL As Single = 0

            Me.m_sHeight = CSng(Math.Sqrt(Me.SumB / sTanHalfAngleRad))
            Me.m_sWidth = CSng(Me.m_sHeight * sTanHalfAngleRad) * 2
            ReDim Me.m_asValues(Me.NumTL)

            '' Diagnostics
            'Console.WriteLine("Pyramid surface {0}, height {1}, width {2} (top angle {3})", SumB, Me.m_sHeight, Me.m_sWidth, sAngleDeg)

            ' Level positions are calculated via the total value of each TL
            For iTL As Integer = Me.NumTL - 1 To 0 Step -1
                sSumTL += Me.m_asTRB(iTL)
                ' TL floor calculated relative to pyramid height
                Me.m_asValues(iTL) = CSng(Math.Sqrt(sSumTL / sTanHalfAngleRad) / Me.m_sHeight)
                '' Diagnostics
                'Console.WriteLine("  TL {0} at {1}", iTL, Me.m_asLevels(iTL))
            Next

        End Sub

        Private Sub Clear()
            Me.m_bValid = False
            Me.m_strModel = ""
            Me.m_pyramidtype = 0
            Me.m_iNumTLMax = 0
            Me.m_strUnit = ""
            Me.m_sTotal = 0.0!
        End Sub

#End Region ' Internals

    End Class

End Namespace
