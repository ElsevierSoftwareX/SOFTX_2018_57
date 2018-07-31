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

Option Explicit On
Option Strict On

Imports System.Drawing.Drawing2D
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports ScientificInterface.Other
Imports EwECore
Imports EwEUtils.Core

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form providing the interface to sketch fishing mortality.
    ''' </summary>
    ''' <remarks>
    ''' <para>VC email 18/04/2009:</para>
    ''' <para>The option of having effort or F as input is confusing people (I 
    ''' clearly saw in Fish 501). Also, when one modifies F it is not saved, but 
    ''' overwritten with effort. That’s OK.</para>
    ''' <para>We need to keep the code for running F as input as this is useful 
    ''' when reading time series. However, we do not need to have it as in input. 
    ''' I’ve discussed this with Carl, and he agrees. Changes to be made:</para>
    ''' <list type="number">
    ''' <item>On Ecosim input menu, Move “fishing effort” to be at the level of 
    ''' Egg production, and placed just below it. Delete the Fishing mortality 
    ''' (under Fishing rate)</item>
    ''' <item>On Ecosim output; run Ecosim: keep the ‘target’ but when users 
    ''' select a Group to see the grey fishing mortalities, these should not be 
    ''' modifiable, just for showing. (One should still be able to edit the 
    ''' fishing efforts by fleets though)</item>
    '''</list>
    ''' <para>That’s all. So from now on, F is either calculated from effort, 
    ''' or read in via a time series. This should reduce confusion.</para>
    ''' <para>JS reply 18/04/2009:</para>
    ''' <para>Should fishing mortality move to the Ecosim output nodes as a 
    ''' read-only form?</para>
    ''' <para>VS reply 19/04/2009:</para>
    ''' <para>Good idea, so yes.</para>
    ''' <para>JS 18/02/2011: F sketching enabled again after repeated requests.</para>
    ''' </remarks>
    ''' =======================================================================
    Public Class frmFishingMortality

#Region " Private variables "

        Private m_handler As cFishingMortalityShapeGUIHandler = Nothing

#End Region ' Private variables

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructors

#Region " Overrides "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            If (Me.UIContext Is Nothing) Then Return

            Me.m_handler = New cFishingMortalityShapeGUIHandler(Me.UIContext)
            Me.m_handler.Attach(Me.m_shapeToolBox, Me.m_shapeToolboxToolbar, _
                                Me.m_sketchPad, Me.m_sketchPadToolbar)
            Me.CoreComponents = New eCoreComponentType() {eCoreComponentType.ShapesManager}
        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)
            Me.m_handler.Detach()
            Me.CoreComponents = Nothing
            MyBase.OnFormClosed(e)
        End Sub

        Public Overrides Sub OnCoreMessage(ByVal msg As EwECore.cMessage)
            Select Case msg.Source
                Case eCoreComponentType.ShapesManager
                    If (msg.DataType = eDataTypes.FishMort) Then
                        Me.m_handler.Refresh()
                    End If
            End Select
        End Sub

#End Region ' Overrides

    End Class

End Namespace


