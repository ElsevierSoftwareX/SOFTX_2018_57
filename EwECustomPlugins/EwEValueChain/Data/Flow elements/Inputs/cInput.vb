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
Imports EwECore

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' A value that entered a cUnit during processing.
''' </summary>
''' ===========================================================================
Public Class cInput

    Private m_sTons As Single = 0.0!
    Private m_sValue As Single = 1.0!
    Private m_src As cUnit = Nothing

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="sTons">Weight of the product, in tons</param>
    ''' <param name="sValue">Total value of the product.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal src As cUnit, ByVal sTons As Single, ByVal sValue As Single)
        Me.m_src = src
        Me.m_sTons = sTons
        Me.m_sValue = sValue
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the weight of input in tons of this input.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Tons() As Single
        Get
            Return Me.m_sTons
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the total value of this input.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Value() As Single
        Get
            Return Me.m_sValue
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The <see cref="cUnit">source</see> of this unit.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Source As cUnit
        Get
            Return Me.m_src
        End Get
    End Property

End Class
