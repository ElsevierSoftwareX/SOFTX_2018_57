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
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.SpatialData

#End Region ' Imports

Namespace Commands

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Command to edit a spatial temporal dataset.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cEditSpatialDatasetCommand
        Inherits cCommand

        Private m_ds As ISpatialDataSet = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>The name of this command.</summary>
          ''' -----------------------------------------------------------------------
        Public Shared COMMAND_NAME As String = "~editspatialdataset"

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new instance of the <see cref="cBrowserCommand"/> class.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal cmdh As cCommandHandler)
            MyBase.New(cmdh, COMMAND_NAME)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Invokes the command to edit a spatial dataset in the EwE UI.
        ''' </summary>
        ''' <param name="ds"><see cref="ISpatialDataSet"/> to edit.</param>
        ''' -----------------------------------------------------------------------
        Public Overloads Sub Invoke(ds As ISpatialDataSet)
            Me.m_ds = ds
            MyBase.Invoke()
            Me.m_ds = Nothing
        End Sub

        ''' <summary>
        ''' Get the dataset to configure.
        ''' </summary>
        Public ReadOnly Property Dataset() As ISpatialDataSet
            Get
                Return Me.m_ds
            End Get
        End Property

    End Class

End Namespace
