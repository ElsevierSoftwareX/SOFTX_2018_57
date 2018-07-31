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
' Copyright 1991- UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwEPlugin
Imports EwEPlugin.Data
Imports ScientificInterfaceShared.Commands
Imports EwEUtils.Core
Imports EwECore
Imports EwEUtils.Utilities

#End Region ' Imports 

Namespace Ecopath.Input

    ''' <summary>
    ''' Form implementing an interface to provide taxonomy input data.
    ''' </summary>
    Public Class frmTaxonInput

        Private m_bHasSearchEngines As Boolean = False

        Public Sub New()
            MyBase.New()
            Me.InitializeComponent()
            Me.Grid = Me.m_grid
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)

            If Me.UIContext Is Nothing Then Return

            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditTaxa")
            If (cmd IsNot Nothing) Then cmd.AddControl(Me.m_tsbnEditTaxa)

            Dim pm As cPluginManager = Me.Core.PluginManager
            Dim pi As IPlugin = Nothing
            Dim dpi As IDataSearchProducerPlugin = Nothing
            Dim coll As ICollection(Of IPlugin) = Nothing

            Me.m_bHasSearchEngines = False

            If (pm Is Nothing) Then Return

            coll = pm.GetPlugins(GetType(IDataSearchProducerPlugin))

            ' Only show data producers that provide taxon data
            For Each pi In coll
                Try
                    dpi = DirectCast(pi, IDataSearchProducerPlugin)
                    If (dpi.IsDataAvailable(GetType(ITaxonSearchData))) Then
                        Dim img As Image = Nothing
                        If (TypeOf dpi Is IGUIPlugin) Then
                            img = DirectCast(dpi, IGUIPlugin).ControlImage
                        End If
                        Dim tsi As ToolStripItem = Me.m_tscmbUpdate.DropDownItems.Add(dpi.Name, img, AddressOf OnClickEngine)
                        tsi.Tag = dpi
                        Me.m_bHasSearchEngines = True
                    End If
                Catch ex As Exception

                End Try
            Next

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As System.Windows.Forms.FormClosedEventArgs)

            If Me.UIContext Is Nothing Then Return

            Dim cmd As cCommand = Me.CommandHandler.GetCommand("EditTaxa")
            If (cmd IsNot Nothing) Then cmd.RemoveControl(Me.m_tsbnEditTaxa)

            Me.StopRefreshTaxa()

            MyBase.OnFormClosed(e)

        End Sub

        Protected Overrides Sub UpdateControls()

            MyBase.UpdateControls()
            Me.m_tscmbUpdate.Enabled = Me.m_bHasSearchEngines

        End Sub

        Private Sub OnClickEngine(sender As Object, args As EventArgs)

            Dim item As ToolStripItem = DirectCast(sender, ToolStripItem)
            Dim engine As IDataSearchProducerPlugin = DirectCast(item.Tag, IDataSearchProducerPlugin)
            Dim taxa() As cTaxon = Me.m_grid.SelectedTaxa()

            If (engine Is Nothing) Then Return
            If (taxa Is Nothing) Then Return
            If (taxa.Count = 0) Then Return

            ' If Not engine.IsEnabled Then Return
            If Not Me.ConfigureProducer(engine) Then Return

            Me.RefreshTaxa(engine, Me.m_grid.SelectedTaxa)

        End Sub

        Private Function ConfigureProducer(prod As IDataSearchProducerPlugin) As Boolean

            Dim ui As Control = Nothing
            If (prod Is Nothing) Then Return False
            If Not (TypeOf prod Is IConfigurable) Then Return True
            Dim cfg As IConfigurable = DirectCast(prod, IConfigurable)

            ' Must be able to configure again!
            'If cfg.IsConfigured Then Return True 

            Try
                ui = DirectCast(prod, IConfigurablePlugin).GetConfigUI()
            Catch ex As Exception
                ui = Nothing
            End Try

            If (ui Is Nothing) Then Return True

            Dim dlg As New dlgConfig(Me.UIContext)
            dlg.ShowDialog(cStringUtils.Localize("Configuring {0}", ui.Text), ui)

        End Function

        Private m_taxaRefresh As cTaxon() = Nothing
        Private m_taxaRefreshEngine As IDataSearchProducerPlugin = Nothing
        Private m_thread As Threading.Thread = Nothing

        Private Sub RefreshTaxa(engine As IDataSearchProducerPlugin, taxa As cTaxon())

            Me.StopRefreshTaxa()

            Me.m_taxaRefresh = taxa
            Me.m_taxaRefreshEngine = engine

            If (engine Is Nothing) Then Return
            If (taxa Is Nothing) Then Return
            If (taxa.Count = 0) Then Return

            Me.m_thread = New Threading.Thread(AddressOf RefreshTaxaThreaded)
            Me.m_thread.Start()

        End Sub

        Private Sub RefreshTaxaThreaded()

            Dim i As Integer = 1

            Me.Core.SetStopRunDelegate(New cCore.StopRunDelegate(AddressOf StopRefreshTaxa))
            Try
                cApplicationStatusNotifier.StartProgress(Me.Core, "Refreshing taxa...")
                For Each taxon As cTaxon In Me.m_taxaRefresh
                    cApplicationStatusNotifier.UpdateProgress(Me.Core, "Refreshing taxon " & i, CSng(1 / Me.m_taxaRefresh.Count))
                    If Me.m_taxaRefreshEngine.StartSearch(taxon, 1) Then
                        ' plop
                        For j As Integer = 1 To 1000000
                            Dim k As Double = Math.Log10(j) * Math.Log(i)
                        Next
                    End If
                    i += 1
                Next
            Catch ex As Exception

            End Try

            Me.m_thread = Nothing
            Me.StopRefreshTaxa()

        End Sub

        Private Sub StopRefreshTaxa()

            Try
                If (Me.m_thread IsNot Nothing) Then
                    Me.m_thread.Abort()
                    Me.m_thread = Nothing
                End If
            Catch ex As Exception
                ' Hiepa
            End Try

            cApplicationStatusNotifier.EndProgress(Me.Core)
            Me.m_taxaRefreshEngine = Nothing
            Me.m_taxaRefresh = Nothing
            Me.Core.SetStopRunDelegate(Nothing)

        End Sub

    End Class

End Namespace