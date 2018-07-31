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
Imports System.Drawing
Imports System.Xml
Imports EwEUtils.Core
Imports EwEUtils.SpatialData

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Placeholder class for a spatial temporal data connection that cannot be
''' initialized, most likely due to a missing plug-in.
''' </summary>
''' -----------------------------------------------------------------------
Friend NotInheritable Class cSpatialDatasetPlaceholder
    Implements ISpatialDataSet

#Region " Private vars "

    Private m_cfg As String = ""

#End Region ' Private vars

#Region " Implementation "

    ''' <inheritdocs cref="ISpatialDataSet.DisplayName"/>
    Public Property DisplayName As String Implements ISpatialDataSet.DisplayName

    ''' <inheritdocs cref="ISpatialDataSet.DataDescription"/>
    Public Property DataDescription As String Implements ISpatialDataSet.DataDescription

    ''' <inheritdocs cref="ISpatialDataSet.Source"/>
    Public Property Source As String Implements ISpatialDataSet.Source

    ''' <inheritdocs cref="ISpatialDataSet.GUID"/>
    Public Property GUID As Guid Implements ISpatialDataSet.GUID

    ''' <inheritdocs cref="ISpatialDataSet.TimeStart"/>
    Public ReadOnly Property TimeStart As Date Implements ISpatialDataSet.TimeStart
        Get
            Return Date.MinValue
        End Get
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.TimeEnd"/>
    Public ReadOnly Property TimeEnd As Date Implements ISpatialDataSet.TimeEnd
        Get
            Return Date.MaxValue
        End Get
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.VarName"/>
    Public Property VarName As eVarNameFlags Implements ISpatialDataSet.VarName

    ''' <inheritdocs cref="ISpatialDataSet.ConversionFormat"/>
    Public ReadOnly Property ConversionFormat As String Implements ISpatialDataSet.ConversionFormat
        Get
            Return ""
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <inheritdocs cref="ISpatialDataSet.Configuration"/>
    ''' -------------------------------------------------------------------
    Public Property Configuration(ByVal doc As XmlDocument,
                                      ByVal strFolderRoot As String) As XmlNode _
            Implements ISpatialDataSet.Configuration
        Get
            Return Me.ToXML(doc, strFolderRoot)
        End Get
        Set(ByVal value As XmlNode)
            Me.FromXML(doc, value, strFolderRoot)
        End Set
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.DialogReadFilter"/>
    Public ReadOnly Property DialogReadFilter(bRaster As Boolean, bImage As Boolean, bVector As Boolean) As String Implements ISpatialDataSet.DialogReadFilter
        Get
            Return ""
        End Get
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.Cache"/>
    Public Property Cache As ISpatialDataCache Implements ISpatialDataSet.Cache

    ''' <inheritdocs cref="ISpatialDataSet.EnableData"/>
    Public Property EnableData(runtype As IRunType) As Boolean Implements IExternalDataSource.EnableData
        Get
            Return False
        End Get
        Set(value As Boolean)
            ' NOP
        End Set
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.Summary"/>
    Public ReadOnly Property Summary As String Implements ISummarizable.Summary
        Get
            Return ""
        End Get
    End Property

    ''' <inheritdocs cref="ISpatialDataSet.UpdateIndexAtT"/>
    Public Sub UpdateIndexAtT(datetime As Date) Implements ISpatialDataSet.UpdateIndexAtT
        ' NOP
    End Sub

    ''' <inheritdocs cref="ISpatialDataSet.IsConfigured"/>
    Public Function IsConfigured() As Boolean Implements ISpatialDataSet.IsConfigured
        Return False
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.HasDataAtT"/>
    Public Function HasDataAtT(datetime As Date) As Boolean Implements ISpatialDataSet.HasDataAtT
        Return False
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.LockDataAtT"/>
    Public Function LockDataAtT(datetime As Date, dCellSize As Double, ptfNE As PointF, ptfSW As PointF, strProjectionString As String) As Boolean Implements ISpatialDataSet.LockDataAtT
        Return False
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.IsLocked"/>
    Public Function IsLocked() As Boolean Implements ISpatialDataSet.IsLocked
        Return False
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.Unlock"/>
    Public Function Unlock() As Boolean Implements ISpatialDataSet.Unlock
        Return True
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.GetExtentAtT"/>
    Public Function GetExtentAtT(datetime As Date, ByRef ptfNW As PointF, ByRef ptfSE As PointF) As Boolean Implements ISpatialDataSet.GetExtentAtT
        Return False
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.IndexStatusAtT"/>
    Public Function IndexStatusAtT(datetime As Date) As ISpatialDataSet.eIndexStatus Implements ISpatialDataSet.IndexStatusAtT
        Return ISpatialDataSet.eIndexStatus.NotIndexed
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.GetAttributes"/>
    Public Function GetAttributes() As String() Implements ISpatialDataSet.GetAttributes
        Return Nothing
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.GetAttributeDataTypes"/>
    Public Function GetAttributeDataTypes() As Type() Implements ISpatialDataSet.GetAttributeDataTypes
        Return Nothing
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.GetAttributeValues"/>
    Public Function GetAttributeValues() As DataTable Implements ISpatialDataSet.GetAttributeValues
        Return Nothing
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.GetRaster"/>
    Public Function GetRaster(converter As ISpatialDataConverter, strLayerName As String) As ISpatialRaster Implements ISpatialDataSet.GetRaster
        Return Nothing
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.ExportTo"/>
    Public Function ExportTo(strPath As String) As ISpatialDataSet Implements ISpatialDataSet.ExportTo
        Return Nothing
    End Function

    ''' <inheritdocs cref="ISpatialDataSet.IsDataAvailable"/>
    Public Function IsDataAvailable(runtype As IRunType) As Boolean Implements IExternalDataSource.IsDataAvailable
        Return False
    End Function

#End Region ' Implementation

#Region " Configuration "

    Public Property PreservedType As String = ""

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Write content to XML.
    ''' </summary>
    ''' <param name="doc">The doc to generate nodes for.</param>
    ''' <returns>
    ''' An XML node that contains the content of the dataset.
    ''' </returns>
    ''' -------------------------------------------------------------------
    Private Function ToXML(ByVal doc As XmlDocument, ByVal strFolderRoot As String) As XmlNode

        Dim xnMaster As XmlNode = Nothing

        xnMaster = doc.CreateElement("Configuration")
        xnMaster.InnerXml = Me.m_cfg
        Return xnMaster

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Read content from XML.
    ''' </summary>
    ''' <param name="doc">The doc to read nodes from.</param>
    ''' <param name="node">The configuration node that contains the content
    ''' of the dataset. Happy, happy, happy.</param>
    ''' <returns>
    ''' True if successful.
    ''' </returns>
    ''' -------------------------------------------------------------------
    Private Function FromXML(ByVal doc As XmlDocument,
                             ByVal node As XmlNode,
                             ByVal strFolderRoot As String) As Boolean

        Dim xn As XmlNode = Nothing
        Dim cin As cCoreEnumNamesIndex = cCoreEnumNamesIndex.GetInstance()

        If (String.Compare(node.Name, "Configuration") <> 0) Then Return False

        Me.m_cfg = node.InnerText

        Try
            For Each xn In node.ChildNodes
                Select Case xn.Name
                    Case "Name"
                        Me.DisplayName = xn.InnerText
                    Case "Description"
                        Me.DataDescription = xn.InnerText
                    Case "Variable"
                        Me.VarName = cin.GetVarName(xn.InnerText)
                End Select
            Next

        Catch ex As Exception
            Return False
        End Try

        Return True

    End Function

#End Region ' Configuration

End Class
