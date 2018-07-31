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
Imports EwEPlugin
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources

#End Region ' Imports

Namespace Other

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Auto-save UI item engine. This engine creates a hierarchy of 
    ''' <see cref="ucAutosaveOption"/> controls that reflect the various
    ''' components in EwE that support auto-save functionality.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Friend Class cAutoSaveItemEngine
        Implements IDisposable

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_pl As Panel = Nothing
        Private m_cbh As cCheckboxHierarchy = Nothing
        Private m_lControls As List(Of ucAutosaveOption) = Nothing

#End Region ' Private vars

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class to sort plug-ins by name.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cPluginSorter
            Implements IComparer(Of IAutoSavePlugin)

            Public Function Compare(x As EwEPlugin.IAutoSavePlugin,
                                y As EwEPlugin.IAutoSavePlugin) As Integer _
                            Implements IComparer(Of EwEPlugin.IAutoSavePlugin).Compare
                Return String.Compare(x.Name, y.Name)
            End Function

        End Class

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="uic">The <see cref="cUIContext"/> to connect to.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext)
            Me.m_uic = uic
            Me.m_lControls = New List(Of ucAutosaveOption)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Disposal.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Detach()
            Me.m_cbh.Dispose()
            GC.SuppressFinalize(Me)
        End Sub

#End Region ' Constructor

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attach the engine to a <see cref="Panel"/>. This will create the
        ''' auto-save control hierarchy. Do not forget to call <see cref="Detach"/> 
        ''' to clean up.
        ''' </summary>
        ''' <param name="pl">The <see cref="Panel"/> to create the control
        ''' hierarchy into.</param>
        ''' -------------------------------------------------------------------
        Public Sub Attach(pl As Panel)

            ' Store panel ref
            Me.m_pl = pl

            Dim core As cCore = Me.m_uic.Core
            Dim pm As cPluginManager = core.PluginManager
            Dim lPlugins([Enum].GetValues(GetType(eAutosaveTypes)).Length - 1) As List(Of IAutoSavePlugin)

            ' Build lists of auto-saving plug-ins, per type
            For Each t As eAutosaveTypes In [Enum].GetValues(GetType(eAutosaveTypes))
                lPlugins(t) = New List(Of IAutoSavePlugin)
            Next

            ' Make inventory of autosave plug-ins
            If (pm IsNot Nothing) Then
                For Each pi As IPlugin In pm.GetPlugins(GetType(IAutoSavePlugin))
                    Dim aspi As IAutoSavePlugin = DirectCast(pi, IAutoSavePlugin)
                    lPlugins(aspi.AutoSaveType).Add(aspi)
                Next pi
            End If

            ' Build control tree
            Me.m_pl.SuspendLayout()
            Try
                Me.BuildControlTree(eAutosaveTypes.NotSet, Nothing, 0, lPlugins)
            Catch ex As Exception
                ' Whoah!
            End Try
            Me.m_pl.ResumeLayout()

            ' Start!
            Me.m_cbh.ManageCheckedStates = True

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Detach the engine from the UI.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Detach()

            Me.m_pl.SuspendLayout()
            For Each uc As ucAutosaveOption In Me.m_lControls
                Me.m_pl.Controls.Remove(uc)
            Next
            Me.m_pl.ResumeLayout()
            Me.m_pl = Nothing

            Me.m_lControls.Clear()
            Me.m_cbh.Dispose()
            Me.m_cbh = Nothing

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Update the output mask for file destinations. This mask is used to
        ''' show the preview paths for the individual autosave options.
        ''' </summary>
        ''' <param name="strMask">The mask to set.</param>
        ''' -------------------------------------------------------------------
        Public Sub SetOutputMask(ByVal strMask As String)
            For Each uc As ucAutosaveOption In Me.m_lControls
                uc.SetOutputMask(strMask)
            Next
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Apply control changes to the underlying components.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Apply()
            For Each uc As ucAutosaveOption In Me.m_lControls
                uc.Apply()
            Next
        End Sub

#End Region ' Public access

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Recursive core to build the hierarchy control structure.
        ''' </summary>
        ''' <param name="t"><see cref="eAutosaveTypes"/> to build a node for.</param>
        ''' <param name="cbParent">Parent checkbox, if any.</param>
        ''' <param name="iIndent">Control indentation.</param>
        ''' <param name="lPlugins">2-dimensional list of autosaving plug-ins.</param>
        ''' -------------------------------------------------------------------
        Private Sub BuildControlTree(ByVal t As eAutosaveTypes, _
                                     ByVal cbParent As CheckBox, _
                                     ByVal iIndent As Integer, _
                                     ByVal lPlugins() As List(Of IAutoSavePlugin))

            Dim ctrl As ucAutosaveOption = Nothing

            Select Case t
                Case eAutosaveTypes.NotSet
                    ctrl = New ucAutosaveOption(Me.m_uic, SharedResources.AUTOSAVE_ALL, 0)
                    Me.Add(ctrl, Nothing)
                    Dim cbRoot As CheckBox = ctrl.Checkbox

                    ctrl = New ucAutosaveOption(Me.m_uic, SharedResources.HEADER_ECOPATH, 1)
                    Me.Add(ctrl, cbRoot)
                    Me.BuildControlTree(eAutosaveTypes.Ecopath, ctrl.Checkbox, 2, lPlugins)

                    ctrl = New ucAutosaveOption(Me.m_uic, SharedResources.HEADER_ECOSIM, 1)
                    Me.Add(ctrl, cbRoot)
                    Me.BuildControlTree(eAutosaveTypes.Ecosim, ctrl.Checkbox, 2, lPlugins)

                    ctrl = New ucAutosaveOption(Me.m_uic, SharedResources.HEADER_ECOSPACE, 1)
                    Me.Add(ctrl, cbRoot)
                    Me.BuildControlTree(eAutosaveTypes.Ecospace, ctrl.Checkbox, 2, lPlugins)

                    Me.BuildControlTree(eAutosaveTypes.Ecotracer, cbRoot, 1, lPlugins)
                    Me.Add(lPlugins(eAutosaveTypes.NotSet), cbRoot, 1)

                Case eAutosaveTypes.Ecopath

                    ' JS 130109: do not provide a UI for saving Ecopath estimations
                    '' Add Ecopath estimates node
                    'ctrl = New ucAutosaveOption(Me.m_uic, t, iIndent)
                    'Me.Add(ctrl, parent)

                    ' Add Ecopath plug-ins
                    Me.Add(lPlugins(t), cbParent, iIndent)

                Case eAutosaveTypes.Ecosim

                    ' Add Ecosim nodes
                    Me.BuildControlTree(eAutosaveTypes.EcosimResults, cbParent, iIndent, lPlugins)
                    Me.BuildControlTree(eAutosaveTypes.MonteCarlo, cbParent, iIndent, lPlugins)
                    Me.BuildControlTree(eAutosaveTypes.MSE, cbParent, iIndent, lPlugins)
                    Me.BuildControlTree(eAutosaveTypes.MSY, cbParent, iIndent, lPlugins)
                    ' Add plug-in nodes
                    Me.Add(lPlugins(t), cbParent, iIndent)

                Case eAutosaveTypes.Ecospace

                    ' Add Ecospace map node
                    Me.BuildControlTree(eAutosaveTypes.EcospaceResults, cbParent, iIndent, lPlugins)
                    Me.BuildControlTree(eAutosaveTypes.MPAOpt, cbParent, iIndent, lPlugins)
                    ' Add Ecospace plug-in nodes
                    Me.Add(lPlugins(t), cbParent, iIndent)

                Case eAutosaveTypes.Ecotracer
                    ' Add tracer node
                    ctrl = New ucAutosaveOption(Me.m_uic, t, iIndent)
                    Me.Add(ctrl, cbParent)
                    ' Add tracer plug-in nodes
                    Me.Add(lPlugins(t), ctrl.Checkbox, iIndent)

                Case Else

                    ' Add master node
                    ctrl = New ucAutosaveOption(Me.m_uic, t, iIndent)
                    Me.Add(ctrl, cbParent)
                    ' Add child plug-in nodes
                    Me.Add(lPlugins(t), ctrl.Checkbox, iIndent + 1)

            End Select
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add a <see cref="ucAutosaveOption"/> control to the managed panel.
        ''' </summary>
        ''' <param name="uc">The control to add.</param>
        ''' <param name="parent">The parent checkbox for this control, if any.</param>
        ''' -------------------------------------------------------------------
        Private Sub Add(ByVal uc As ucAutosaveOption, ByVal parent As CheckBox)

            Me.m_pl.Controls.Add(uc)
            uc.Location = New Point(0, (Me.m_pl.Controls.Count - 1) * uc.Height)
            uc.Width = Me.m_pl.Width
            uc.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top

            If (parent IsNot Nothing) Then
                Me.m_cbh.Add(uc.Checkbox, parent)
            Else
                Me.m_cbh = New cCheckboxHierarchy(uc.Checkbox)
            End If

            Me.m_lControls.Add(uc)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add controls for a list of plug-ins.
        ''' </summary>
        ''' <param name="l"></param>
        ''' <param name="parent"></param>
        ''' <param name="iIndent"></param>
        ''' -------------------------------------------------------------------
        Private Sub Add(ByVal l As List(Of IAutoSavePlugin), _
                        ByVal parent As CheckBox, _
                        ByVal iIndent As Integer)

            Dim api As IAutoSavePlugin() = l.ToArray
            Array.Sort(api, New cPluginSorter())
            For Each pi As IAutoSavePlugin In api
                Me.Add(New ucAutosaveOption(Me.m_uic, pi, iIndent), parent)
            Next

        End Sub

#End Region ' Internals

    End Class

End Namespace ' Other
