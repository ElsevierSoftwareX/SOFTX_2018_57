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
Option Explicit On

Imports ZedGraph
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Globalization
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities

#End Region ' Imports

Public Module modUtility

    Public Const DEFAULT_COL_WIDTH As Integer = 70
    Public Const ID_COL_WIDTH As Integer = 25
    Public Const GRP_NAME_COL_WIDTH As Integer = 110
    Public Const FIRST_ROW_HEIGHT As Integer = 45

    Public Sub SetGridColumnPropertyDefault(ByVal DataGrid As System.Windows.Forms.DataGridView)
        DataGrid.ColumnHeadersVisible = False
        For intColIndex As Integer = 0 To DataGrid.ColumnCount - 1
            'DataGrid.Columns(intColIndex).HeaderCell.Style.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
            DataGrid.Columns(intColIndex).DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
            DataGrid.Columns(intColIndex).DefaultCellStyle.BackColor = Drawing.Color.White
            DataGrid.Columns(intColIndex).Width = DEFAULT_COL_WIDTH '110
            DataGrid.Columns(intColIndex).Frozen = False
            DataGrid.Columns(intColIndex).SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Next
    End Sub

    <CLSCompliant(False)> _
    Public Sub AddCurve(ByVal strName As String, ByVal CurveVar() As Single, ByVal Pane As GraphPane, _
      ByVal MyColor As Color, Optional ByVal Symbol As SymbolType = SymbolType.None)
        Dim List As PointPairList
        Dim iNumPoints As Integer = CurveVar.GetUpperBound(0)

        List = New PointPairList()
        For iTime As Integer = 1 To iNumPoints
            List.Add(iTime, CurveVar(iTime))
        Next
        Pane.AddCurve(strName, List, MyColor, Symbol)

        Pane.XAxis.Scale.Max = iNumPoints
    End Sub

    Public Enum ePyramidTypes As Byte
        [Catch] = 0
        Flow = 1
        Biomass = 2
    End Enum

    ''' <summary>
    ''' Enumerator, specifying which groups to show in group filters.
    ''' </summary>
    Public Enum eGroupFilterTypes As Byte
        ''' <summary>Show living groups only</summary>
        Living
        ''' <summary>Show all groups</summary>
        All
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a temporary file for storing a pyramid file.
    ''' </summary>
    ''' <param name="strModel">The EwE model to generate the file for.</param>
    ''' <param name="pyramidtype">The type of pyramid to store in the file.</param>
    ''' <param name="strExtension">The file extension to use.</param>
    ''' <returns>A valid file name in the local temp directory.</returns>
    ''' -----------------------------------------------------------------------
    Public Function PyramidTempFile(ByVal strModel As String, _
                                    ByVal pyramidtype As ePyramidTypes, _
                                    ByVal strExtension As String) As String

        Dim sbFileName As New StringBuilder()

        sbFileName.Append("NA_pyramid_")
        sbFileName.Append(pyramidtype.ToString().ToLower())
        sbFileName.Append("_")
        sbFileName.Append(strModel.ToLower())
        If Not strExtension.StartsWith(".") Then sbFileName.Append(".")
        sbFileName.Append(strExtension)

        Return cFileUtils.MakeTempFile(cFileUtils.ToValidFileName(sbFileName.ToString(), False))

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strModel"></param>
    ''' <param name="pyramidtype"></param>
    ''' <param name="strUnits"></param>
    ''' <param name="iNumTL"></param>
    ''' <param name="sTotalB"></param>
    ''' <param name="asBiomass"></param>
    ''' <param name="asValue"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function WritePyramidFile(ByVal strModel As String, _
                                     ByVal pyramidtype As ePyramidTypes, _
                                     ByVal strUnits As String, _
                                     ByVal iNumTL As Integer, _
                                     ByVal sTotalB As Single, _
                                     ByVal asBiomass() As Single, _
                                     ByVal asValue() As Single) As String

        Dim doc As XmlDocument = New XmlDocument()
        Dim nodePyramid As XmlNode = Nothing
        Dim attrib As XmlAttribute = Nothing
        Dim nodeTL As XmlNode = Nothing
        Dim ciEnUSLocale As New CultureInfo("en-US")
        Dim strOutputFile As String = PyramidTempFile(strModel, pyramidtype, ".xml")

        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", ""))
        nodePyramid = doc.CreateElement("pyramid")
        doc.AppendChild(nodePyramid)

        attrib = doc.CreateAttribute("model")
        attrib.Value = strModel
        nodePyramid.Attributes.Append(attrib)

        attrib = doc.CreateAttribute("type")
        attrib.Value = pyramidtype.ToString()
        nodePyramid.Attributes.Append(attrib)

        attrib = doc.CreateAttribute("unit")
        attrib.Value = strUnits
        nodePyramid.Attributes.Append(attrib)

        attrib = doc.CreateAttribute("total-biomass")
        attrib.Value = sTotalB.ToString(ciEnUSLocale)
        nodePyramid.Attributes.Append(attrib)

        attrib = doc.CreateAttribute("num-tl")
        attrib.Value = iNumTL.ToString(ciEnUSLocale)
        nodePyramid.Attributes.Append(attrib)

        For iTL As Integer = 1 To iNumTL
            nodeTL = doc.CreateElement("trophic-level")

            attrib = doc.CreateAttribute("level")
            attrib.Value = iTL.ToString(ciEnUSLocale)
            nodeTL.Attributes.Append(attrib)

            attrib = doc.CreateAttribute("biomass")
            attrib.Value = asBiomass(iTL).ToString(ciEnUSLocale)
            nodeTL.Attributes.Append(attrib)

            attrib = doc.CreateAttribute("value")
            attrib.Value = asValue(iTL).ToString(ciEnUSLocale)
            nodeTL.Attributes.Append(attrib)

            nodePyramid.AppendChild(nodeTL)
        Next iTL

        doc.Save(strOutputFile)

        Return strOutputFile

    End Function

End Module
