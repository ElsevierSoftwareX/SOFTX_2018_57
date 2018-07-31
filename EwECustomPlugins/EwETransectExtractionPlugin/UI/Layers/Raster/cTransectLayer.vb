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
Imports EwECore
Imports EwEUtils.Core

#End Region ' Imports

''' <summary>
''' Core data layer wrapper for transect raster data.
''' </summary>
Public Class cTransectLayer
    Inherits cEcospaceLayerSingle

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor for an NxN layer of Single values, that derives its data and 
    ''' identity from a manager.
    ''' </summary>
    ''' <param name="core"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal core As cCore, ds As cTransectDatastructures)
        ' Provide a bogus varname (but not NotSet!) as the manager does not care
        MyBase.New(core, ds, "Transect cells", eVarNameFlags.Author)
    End Sub

    Public Overrides Property Cell(iRow As Integer, iCol As Integer, Optional iIndexSec As Integer = -9999) As Object
        Get
            Dim cells As Point() = CType(Me.Manager.LayerData(eVarNameFlags.NotSet, 0), Point())
            If (cells Is Nothing) Then Return cCore.NULL_VALUE
            If Not cells.Contains(New Point(iCol, iRow)) Then Return cCore.NULL_VALUE
            Return 1
        End Get
        Set(value As Object)
            ' NOP
        End Set
    End Property

End Class

