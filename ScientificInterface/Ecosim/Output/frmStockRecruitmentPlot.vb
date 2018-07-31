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

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities.cSystemUtils
Imports EwEUtils.Utilities
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports ZedGraph

#End Region

Namespace Ecosim

    ''' =======================================================================
    ''' <summary>
    ''' Form that implements the stock recruitment plot.
    ''' </summary>
    ''' =======================================================================
    Public Class frmStockRecruitmentPlot

        ' ToDo: add colour rectangle beside group name

#Region " Helper classes "

        ''' <summary>
        ''' Class maintaining a single stock/recruitment value pair.
        ''' </summary>
        Private Class cSRData
            Private m_sStock As Single
            Private m_sRecruitment As Single

            Public Sub New(ByVal sStock As Single, ByVal sRecruitment As Single)
                Me.m_sStock = sStock
                Me.m_sRecruitment = sRecruitment
            End Sub

            Public ReadOnly Property Stock() As Single
                Get
                    Return Me.m_sStock
                End Get
            End Property

            Public ReadOnly Property Recruitment() As Single
                Get
                    Return Me.m_sRecruitment
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_INDEXED, Me.m_sStock, Me.m_sRecruitment)
            End Function
        End Class

        ''' <summary>
        ''' Class maintaining a single line in the SR plot.
        ''' </summary>
        Private Class cSRLine

            Private m_sg As cStanzaGroup
            Private m_grpStart As cCoreGroupBase
            Private m_grpEnd As cCoreGroupBase

            Private m_lData As New List(Of cSRData)
            Private m_bIsVisible As Boolean = False
            Private m_bIsDefault As Boolean = False

            Public Sub New(ByVal sg As cStanzaGroup, ByVal grpStart As cCoreGroupBase, ByVal grpEnd As cCoreGroupBase)
                Me.m_sg = sg
                Me.m_grpStart = grpStart
                Me.m_grpEnd = grpEnd
            End Sub

            Public ReadOnly Property StanzaGroup() As cStanzaGroup
                Get
                    Return Me.m_sg
                End Get
            End Property

            Public ReadOnly Property GroupStart() As cCoreGroupBase
                Get
                    Return Me.m_grpStart
                End Get
            End Property

            Public ReadOnly Property GroupEnd() As cCoreGroupBase
                Get
                    Return Me.m_grpEnd
                End Get
            End Property

            Public ReadOnly Property Data() As List(Of cSRData)
                Get
                    Return Me.m_lData
                End Get
            End Property

            Public Property IsVisible() As Boolean
                Get
                    Return Me.m_bIsVisible
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bIsVisible = value
                End Set
            End Property

            Public Property IsDefault() As Boolean
                Get
                    Return Me.m_bIsDefault
                End Get
                Set(ByVal value As Boolean)
                    Me.m_bIsDefault = value
                End Set
            End Property

            Public ReadOnly Property Title() As String
                Get
                    Return cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, Me.m_grpStart.Name, Me.m_grpEnd.Name)
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return Me.Title
            End Function

        End Class

#End Region ' Helper classes

#Region " Private vars "

        Private m_coreStateMonitor As cCoreStateMonitor = Nothing
        Private m_curveSlope As CurveItem = Nothing
        Private m_mhEcosim As cMessageHandler = Nothing
        Private m_SRResults As List(Of cSRLine)
        Private m_zgh As cZedGraphHelper = Nothing

#End Region ' Private vars

#Region " Constructors "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

#End Region ' Constructors

#Region " Events "

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

            MyBase.OnLoad(e)

            If (Me.UIContext Is Nothing) Then Return

            Me.m_coreStateMonitor = Me.Core.StateMonitor
            Me.m_SRResults = New List(Of cSRLine)
            Me.m_zgh = New cZedGraphHelper()

            Me.LoadGroups()
            Me.m_zgh.Attach(Me.UIContext, Me.m_plot)
            Me.m_zgh.ConfigurePane(My.Resources.SR_PLOT_TITLE,
                                   cStringUtils.Localize(My.Resources.SR_PLOT_X_AXIS, String.Empty),
                                   SharedResources.HEADER_RECRUITMENT,
                                   False)

            Dim m_SyncObj As System.Threading.SynchronizationContext = System.Threading.SynchronizationContext.Current
            'if there is no current context then create a new one on this thread. 
            If (m_SyncObj Is Nothing) Then m_SyncObj = New System.Threading.SynchronizationContext()

            ' Start listening for core messages
            Me.m_mhEcosim = New cMessageHandler(AddressOf EcosimMessageHandler, eCoreComponentType.EcoSim, eMessageType.Any, m_SyncObj)
#If DEBUG Then
            Me.m_mhEcosim.Name = "frmStockRecruitment.Ecosim"
#End If
            Me.Core.Messages.AddMessageHandler(Me.m_mhEcosim)

            AddHandler Me.m_coreStateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

        End Sub

        Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)

            Me.Core.Messages.RemoveMessageHandler(Me.m_mhEcosim)
            Me.m_mhEcosim = Nothing
            Me.m_zgh.Detach()

            RemoveHandler Me.m_coreStateMonitor.CoreExecutionStateEvent, AddressOf OnCoreExecutionStateChanged

            MyBase.OnFormClosed(e)

        End Sub

        ''' <summary>
        ''' Keep me open, please!
        ''' </summary>
        Public Overrides ReadOnly Property IsRunForm() As Boolean
            Get
                Return True
            End Get
        End Property

        Private Sub btnRun_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
            Handles m_btnRun.Click

            Try

                If Not IsRunning Then

                    For i As Integer = 0 To m_SRResults.Count - 1
                        m_SRResults(i).Data.Clear()
                    Next
                    Me.Core.RunEcoSim(AddressOf TimeStepFromEcoSim_handler)
                Else
                    Me.Core.StopEcoSim()
                End If

            Catch ex As Exception

            End Try

        End Sub

        Private Sub TimeStepFromEcoSim_handler(ByVal iTime As Long, ByVal results As cEcoSimResults)

            If results.hasSRData Then
                Try
                    Me.BuildSRData(results)
                Catch ex As Exception

                End Try
            End If

        End Sub

        Private Sub OnCoreExecutionStateChanged(ByVal csm As cCoreStateMonitor)

            ' Check whether ecosim is running
            Dim bEcosimRunning As Boolean = (csm.IsEcosimRunning)

            ' Is this a state change?
            If (bEcosimRunning <> Me.IsRunning) Then
                ' #Yes: update to new state
                Me.IsRunning = bEcosimRunning

                ' Configure run/stop button
                Me.m_btnRun.Text = if(Me.IsRunning, My.Resources.LABEL_STOP, My.Resources.LABEL_RUN)
                Me.m_btnRun.Enabled = Me.m_coreStateMonitor.HasEcosimLoaded
                ' Reflect change immediately
                Me.m_btnRun.Update()

            End If

        End Sub

        Protected Overrides Sub OnStyleGuideChanged(ByVal change As cStyleGuide.eChangeType)
            If (change And cStyleGuide.eChangeType.Colours) > 0 Then
                ' Add the curves again
                Me.AddCurves()
            End If
        End Sub

        Private Sub EcosimMessageHandler(ByRef msg As cMessage)

            Try
                Select Case msg.Type
                    Case eMessageType.EcosimRunCompleted
                        If Not m_SRResults Is Nothing Then
                            Me.AddCurves()
                        End If
                End Select

            Catch ex As Exception
                cLog.Write(ex)
            End Try

        End Sub

        Private Sub tvGroups_AfterSelect(ByVal sender As System.Object, ByVal e As TreeViewEventArgs) _
            Handles m_tvGroups.AfterSelect

            Try

                Dim iLevel As Integer = e.Node.Level
                Select Case iLevel

                    Case 0
                        Me.ShowDefault()

                    Case 1
                        Me.ShowStanza(DirectCast(e.Node.Tag, cStanzaGroup))

                    Case 2
                        ' JS 31Oct10: Enough of this string parsing nonsense
                        Me.ShowSingleGroup(DirectCast(e.Node.Tag, cSRLine))

                End Select

            Catch ex As Exception

            End Try

        End Sub

        Private Function zgSRPlot_MouseDownEvent(ByVal sender As ZedGraph.ZedGraphControl, ByVal e As MouseEventArgs) As System.Boolean _
            Handles m_plot.MouseDownEvent

            Try

                Dim ptMouse As New PointF(e.X, e.Y)
                Dim pane As GraphPane = Me.m_plot.MasterPane.FindChartRect(ptMouse)
                Dim x, y As Double
                Dim item As CurveItem = Nothing
                Dim sg As cStyleGuide = Me.UIContext.StyleGuide

                If Not pane Is Nothing Then

                    pane.ReverseTransform(ptMouse, x, y)
                    item = pane.AddCurve("", New Double() {0.0, x}, New Double() {0.0, y}, Color.Black, SymbolType.None)
                    m_lblPt.Text = cStringUtils.Localize(My.Resources.ECOSIM_SR_SLOPELABEL, _
                                               sg.FormatNumber(CSng(x)), sg.FormatNumber(CSng(y)), _
                                               sg.FormatNumber(CSng(y / x)))
                    RemoveSlopeCurve(item)
                End If

            Catch ex As Exception

            End Try

            Return False
        End Function

        Private Function zgSRPlot_MouseMoveEvent(ByVal sender As ZedGraph.ZedGraphControl, ByVal e As MouseEventArgs) As System.Boolean _
            Handles m_plot.MouseMoveEvent

            Try

                Dim mousePt As New PointF(e.X, e.Y)
                Dim pane As GraphPane = Me.m_plot.MasterPane.FindChartRect(mousePt)

                If pane Is Nothing Then
                    m_lblPt.Text = String.Empty
                    RemoveSlopeCurve(Nothing)
                End If

            Catch ex As Exception

            End Try
            Return True

        End Function

#End Region ' Events

#Region " Internals "

        Private Sub LoadGroups()

            Dim stanza As cStanzaGroup = Nothing
            Dim groupStart As cEcoPathGroupInput = Nothing
            Dim groupEnd As cEcoPathGroupInput = Nothing
            Dim node As TreeNode = Nothing
            Dim iGroupLast As Integer = 0
            Dim iGroup As Integer = 0
            Dim srl As cSRLine = Nothing

            m_tvGroups.BeginUpdate()
            m_tvGroups.Nodes.Clear()

            m_SRResults.Clear()

            If Me.Core.nStanzas > 0 Then
                m_tvGroups.Nodes.Add(SharedResources.HEADER_SHOWALL)

                'Stanza group index is Zero-based.
                For i As Integer = 0 To Me.Core.nStanzas - 1
                    ' Get stanza group
                    stanza = Me.Core.StanzaGroups(i)
                    ' Add stanza node
                    node = New TreeNode(stanza.Name)
                    node.Tag = stanza
                    m_tvGroups.Nodes(0).Nodes.Add(node)

                    ' Add subnodes for life stages
                    iGroupLast = stanza.iGroups(stanza.nLifeStages)
                    groupEnd = Me.Core.EcoPathGroupInputs(iGroupLast)

                    For j As Integer = 1 To stanza.nLifeStages - 1

                        iGroup = stanza.iGroups(j)
                        groupStart = Me.Core.EcoPathGroupInputs(iGroup)

                        srl = New cSRLine(stanza, groupStart, groupEnd)
                        srl.IsDefault = (j = 1)
                        srl.IsVisible = srl.IsDefault

                        node = New TreeNode(srl.Title)
                        node.Tag = srl
                        m_tvGroups.Nodes(0).Nodes(i).Nodes.Add(node) ' Wow, here's to having some good faith....

                        m_SRResults.Add(srl)

                    Next
                Next
                m_btnRun.Enabled = True
            Else
                m_tvGroups.Nodes.Add(My.Resources.SR_PLOT_NO_STANZA_GROUP)
                m_btnRun.Enabled = False
            End If

            m_tvGroups.EndUpdate()
            m_tvGroups.ExpandAll()

        End Sub

        Private Sub BuildSRData(ByVal results As cEcoSimResults)

            Dim stanza As cStanzaGroup = Nothing
            Dim group As cEcoPathGroupInput = Nothing
            Dim tmpSR As cSRData = Nothing

            For i As Integer = 1 To results.nStanza
                stanza = Me.Core.StanzaGroups(i - 1)
                For j As Integer = 1 To stanza.nLifeStages - 1
                    group = Me.Core.EcoPathGroupInputs(stanza.iGroups(j))
                    If results.hasSRData(i, j) Then
                        tmpSR = New cSRData(results.BStock(i, j), results.BRecruitment(i, j))
                        For k As Integer = 0 To m_SRResults.Count - 1
                            If (ReferenceEquals(stanza, Me.m_SRResults(k).StanzaGroup)) And _
                               (ReferenceEquals(group, Me.m_SRResults(k).GroupStart)) Then
                                Me.m_SRResults(k).Data.Add(tmpSR)
                                Exit For
                            End If
                        Next
                    End If
                Next
            Next

            Me.AddCurves()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Add the curves to the pane.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub AddCurves()

            Me.m_plot.GraphPane.CurveList.Clear()

            Dim curve As CurveItem = Nothing
            Dim ppl As PointPairList = Nothing
            Dim srl As cSRLine = Nothing
            Dim srd As cSRData = Nothing

            For i As Integer = 0 To m_SRResults.Count - 1

                srl = Me.m_SRResults(i)
                ppl = New PointPairList()

                For j As Integer = 0 To m_SRResults(i).Data.Count - 1
                    srd = srl.Data(j)
                    ppl.Add(srd.Stock, srd.Recruitment)
                Next

                curve = Me.m_plot.GraphPane.AddCurve(srl.Title, ppl, _
                                      Me.StyleGuide.GroupColor(Me.Core, srl.GroupStart.Index), _
                                      SymbolType.Circle)

                curve.IsVisible = srl.IsVisible

            Next

            Me.m_plot.AxisChange()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strTitleX"></param>
        ''' <param name="strTitleY"></param>
        ''' -------------------------------------------------------------------
        Private Sub UpdateCurves(ByVal strTitleX As String, ByVal strTitleY As String)

            Dim line As cSRLine = Nothing
            Dim curve As CurveItem = Nothing

            For i As Integer = 0 To m_SRResults.Count - 1
                line = Me.m_SRResults(i)
                curve = Me.m_plot.GraphPane.CurveList(line.Title)

                If (curve IsNot Nothing) Then
                    curve.IsVisible = line.IsVisible
                End If
            Next

            Me.m_plot.GraphPane.XAxis.Title.Text = cStringUtils.Localize(My.Resources.SR_PLOT_X_AXIS, strTitleX)
            Me.m_plot.GraphPane.YAxis.Title.Text = cStringUtils.Localize(SharedResources.GENERIC_LABEL_DETAILED, SharedResources.HEADER_RECRUITMENT, strTitleY)

            Me.m_plot.AxisChange()
            Me.m_plot.Refresh()

        End Sub

        Private Sub ShowDefault()
            For i As Integer = 0 To m_SRResults.Count - 1
                Dim srl As cSRLine = Me.m_SRResults(i)
                srl.IsVisible = srl.IsDefault
            Next
            Me.UpdateCurves("", "")
        End Sub

        Private Sub ShowStanza(ByVal stanzaGroup As cStanzaGroup)
            For i As Integer = 0 To m_SRResults.Count - 1
                Dim srl As cSRLine = Me.m_SRResults(i)
                srl.IsVisible = ReferenceEquals(m_SRResults(i).StanzaGroup, stanzaGroup)
            Next
            Me.UpdateCurves("", "")
        End Sub

        Private Sub ShowSingleGroup(ByVal srlShow As cSRLine)
            Dim strTitleX As String = ""
            Dim strTitleY As String = ""
            Dim srl As cSRLine = Nothing

            For i As Integer = 0 To m_SRResults.Count - 1
                srl = Me.m_SRResults(i)
                If ReferenceEquals(srlShow, srl) Then
                    srl.IsVisible = True
                    strTitleX = srl.GroupEnd.Name
                    strTitleY = srl.GroupStart.Name
                Else
                    srl.IsVisible = False
                End If
            Next
            Me.UpdateCurves(strTitleX, strTitleY)
        End Sub

        Private Sub RemoveSlopeCurve(ByVal item As CurveItem)

            If Not m_curveSlope Is Nothing Then
                Me.m_plot.GraphPane.CurveList.Remove(Me.m_curveSlope)
            End If
            Me.m_curveSlope = item
            Me.m_plot.Refresh()

        End Sub

#End Region 'Internals

    End Class

End Namespace
