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
Imports System.Windows.Forms
Imports EwECore
Imports EwECore.WebServices
Imports EwEPlugin
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Controls

#End Region ' Imports

Public Class cEcotrophPlugin
    Implements EwEPlugin.IGUIPlugin
    Implements EwEPlugin.IMenuItemPlugin
    Implements EwEPlugin.ICorePlugin
    Implements EwEPlugin.IEcopathRunCompletedPlugin
    Implements EwEPlugin.IHelpPlugin
    Implements EwEPlugin.IUIContextPlugin

    Public Sub New()

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public ReadOnly Property HelpTopic As String Implements EwEPlugin.IHelpPlugin.HelpTopic
        Get
            Return "http://sirs.agrocampus-ouest.fr//EcoTroph/index.php?action=examples"
        End Get
    End Property

    Public ReadOnly Property HelpURL As String Implements EwEPlugin.IHelpPlugin.HelpURL
        Get
            Return Me.HelpTopic
        End Get
    End Property

    Public Shared ETinputdata As ETinputtot
    Public Shared ETinputdatafromEP As ETinputtot
    ' Public Shared ETinputdataFLEET As ETinputFLEET
    ' Public Shared ETinputdataFLEETfromEP As ETinputFLEET
    Public Shared etCore As cCore
    Public Shared pack_version As String

    Private m_uic As cUIContext

    Private frmET As frmEcotroph

    Public Sub CoreInitialized(ByRef objEcoPath As Object, ByRef objEcoSim As Object, ByRef objEcoSpace As Object) Implements EwEPlugin.ICorePlugin.CoreInitialized
        ETinputdata = New ETinputtot()
        ETinputdatafromEP = New ETinputtot()
    End Sub

    Public ReadOnly Property Author As String Implements EwEPlugin.IPlugin.Author
        Get
            Return "Jerome Guitton, Didier Gascuel"
        End Get
    End Property

    Public ReadOnly Property Contact As String Implements EwEPlugin.IPlugin.Contact
        Get
            Return "jerome.guitton@agrocampus-ouest.fr"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements EwEPlugin.IPlugin.Description
        Get
            Return "EcoTroph (ET) is a modelling approach articulated around the idea that an ecosystem can be represented by its biomass distribution across trophic levels. Such an approach, wherein species as such disappear, may be regarded as the ultimate stage in the use of the trophic level metric for ecosystem modelling. By concentrating on biomass flow as a quasi-physical process, it allows aspects of ecosystem functioning to be explored which are complementary to EwE. It provides users with simple tools to quantify the impacts of fishing at an ecosystem scale and a new way of looking at ecosystems. It thus appears a useful complement to Ecopath."
        End Get
    End Property

    Public Sub Initialize(ByVal core As Object) Implements EwEPlugin.IPlugin.Initialize
        Try
            etCore = DirectCast(core, cCore)
        Catch ex As Exception
            cLog.Write(ex, "cEcotrophPlugin.Initialize")
        End Try
    End Sub

    Public ReadOnly Property Name As String Implements EwEPlugin.IPlugin.Name
        Get
            Return "ET plug-in"
        End Get
    End Property

    Public ReadOnly Property ControlImage As System.Drawing.Image Implements EwEPlugin.IGUIPlugin.ControlImage
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property ControlText As String Implements EwEPlugin.IGUIPlugin.ControlText
        Get
            Return "EcoTroph"
        End Get
    End Property

    Public ReadOnly Property ControlTooltipText As String Implements EwEPlugin.IGUIPlugin.ControlTooltipText
        Get
            Return "EcoTroph"
        End Get
    End Property

    Public ReadOnly Property EnabledState As EwEUtils.Core.eCoreExecutionState Implements EwEPlugin.IGUIPlugin.EnabledState
        Get
            Return EwEUtils.Core.eCoreExecutionState.Idle
        End Get
    End Property

    Public Sub OnControlClick(ByVal sender As Object, ByVal e As System.EventArgs, ByRef frmPlugin As System.Windows.Forms.Form) Implements EwEPlugin.IGUIPlugin.OnControlClick

        Try
            If Not Me.HasInterface(Me.frmET) Then
                frmET = New frmEcotroph()
                frmET.UIContext = Me.m_uic
            End If

            ' Pass form reference back to calling app
            frmPlugin = frmET

        Catch ex As Exception
            cLog.Write(ex, "cEcotrophPlugin.OnControlClick")
        End Try
    End Sub


    Private Function HasInterface(ByVal theForm As System.Windows.Forms.Form) As Boolean
        If theForm Is Nothing Then Return False
        If theForm.IsDisposed Then Return False
        Return True
    End Function

    Public ReadOnly Property MenuItemLocation As String Implements EwEPlugin.IMenuItemPlugin.MenuItemLocation
        Get
            Return "MenuTools"
        End Get
    End Property

    Public Sub EcopathRunCompleted(ByRef EcopathDataStructures As Object) Implements EwEPlugin.IEcopathRunCompletedPlugin.EcopathRunCompleted

        Try
            Dim epdata As EwECore.cEcopathDataStructures
            Dim compteur As Integer
            epdata = DirectCast(EcopathDataStructures, cEcopathDataStructures)

            Dim default_accessibility As Single = 0.8

            ReDim ETinputdatafromEP.B(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.GroupName(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.PROD(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.TL(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.accessibility(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.OI(epdata.B.Length - 1)
            ReDim ETinputdatafromEP.FleetName(epdata.NumFleet)

            ReDim ETinputdata.B(epdata.B.Length - 1)
            ReDim ETinputdata.GroupName(epdata.B.Length - 1)
            ReDim ETinputdata.PROD(epdata.B.Length - 1)
            ReDim ETinputdata.TL(epdata.B.Length - 1)
            ReDim ETinputdata.accessibility(epdata.B.Length - 1)
            ReDim ETinputdata.OI(epdata.B.Length - 1)
            ReDim ETinputdata.FleetName(epdata.NumFleet)

            System.Array.Copy(epdata.B, ETinputdatafromEP.B, epdata.B.Length)
            System.Array.Copy(epdata.GroupName, ETinputdatafromEP.GroupName, epdata.GroupName.Length)
            System.Array.Copy(epdata.PB, ETinputdatafromEP.PROD, epdata.PB.Length)
            ' Rajout du search and replace pour les production, pour mettre à 0 les valeurs ecopath à -9999
            For compteur = 0 To UBound(ETinputdatafromEP.PROD)
                If ETinputdatafromEP.PROD(compteur) = -9999 Then ETinputdatafromEP.PROD(compteur) = 0
            Next

            System.Array.Copy(epdata.TTLX, ETinputdatafromEP.TL, epdata.TTLX.Length)
            System.Array.Copy(epdata.FleetName, ETinputdatafromEP.FleetName, epdata.NumFleet + 1)

            'Récupération de l'index d'Omnivory
            System.Array.Copy(epdata.BQB, ETinputdatafromEP.OI, epdata.BQB.Length)
            ETinputdatafromEP.NumFleet = epdata.NumFleet
            ETinputdatafromEP.Catches = New Single(epdata.NumFleet)() {}
            ETinputdata.Catches = New Single(epdata.NumFleet)() {}
            'ETinputdata.comments = 

            ETinputdata.ModelName = epdata.ModelName
            ETinputdata.ModelDescription = epdata.ModelDescription


            For ifleet As Integer = 0 To epdata.NumFleet - 1
                ETinputdata.FleetName(ifleet) = epdata.FleetName(ifleet + 1)
                ETinputdatafromEP.Catches(ifleet) = New Single(epdata.GroupName.Length) {}
                ETinputdata.Catches(ifleet) = New Single(epdata.GroupName.Length) {}
                For j As Integer = 1 To epdata.B.Length - 1
                    If (ETinputdatafromEP.accessibility(j) = 0 And (epdata.Landing(ifleet, j) > 0 Or epdata.Discard(ifleet, j) > 0)) Then ETinputdatafromEP.accessibility(j) = default_accessibility
                    ETinputdatafromEP.Catches(ifleet)(j) = epdata.Landing(ifleet + 1, j) + epdata.Discard(ifleet + 1, j)


                Next
            Next

        Catch ex As Exception
            cLog.Write(ex, "cEcotrophPlugin.EcopathRunCompleted")
        End Try

    End Sub

    'Private Function match(ByVal epdata As cEcopathDataStructures, ByVal p2 As String) As Array
    '    Throw New NotImplementedException
    'End Function

    Public Sub UIContext(uic As Object) Implements EwEPlugin.IUIContextPlugin.UIContext
        Try
            Me.m_uic = DirectCast(uic, cUIContext)
        Catch ex As Exception
            cLog.Write(ex, "cEcotrophPlugin.UIContext")
        End Try
    End Sub

End Class
