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
Imports System.Drawing.Drawing2D
Imports EwECore
Imports EwECore.SpatialData
Imports EwEUtils.Core
Imports EwEUtils.SpatialData
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands

#End Region ' Imports

Namespace Ecospace.Controls

    Public Class ucSpatialTimeSeriesToolbox
        Implements IUIElement

#Region " Private classes "

        ''' <summary>
        ''' Helper administration class for a data set in the toolbox
        ''' </summary>
        Private Class cDatasetInfo
            Public m_ds As ISpatialDataSet
            Public m_var As eVarNameFlags
            Public m_guid As Guid
            Public m_iTimeStart As Integer = 0
            Public m_iTimeEnd As Integer = 0
            Public m_iPosVert As Integer = 0
            Public m_liData As New List(Of Integer) ' Time steps with data
            Public m_liTime As New List(Of DateTime) ' Translated time for time steps
        End Class

#End Region ' Private classes

#Region " Private vars "

        ' Formatting constants
        Private Const c_headerheight As Integer = 18
        Private Const c_barheight As Integer = 24
        Private Const c_barlabelheight As Integer = 18
        Private Const c_barmargin As Integer = 3
        Private Const c_dotradius As Integer = 2
        Private Const c_imgradius As Integer = 4

        Private m_uic As cUIContext = Nothing
        Private m_varname As eVarNameFlags = eVarNameFlags.NotSet
        Private m_lInfo As New List(Of cDatasetInfo)
        Private m_iTimestepSize As Integer = 0 ' Will be calculated, should perhaps be configurable
        Private m_iSelectedIndex As Integer = -1
        Private m_iSelectedTimeStep As Integer = -1

        Private m_mhSpace As cMessageHandler = Nothing
        Private m_mhSpatial As cMessageHandler = Nothing

        Private m_manSets As cSpatialDataSetManager = Nothing

        Private m_bmpError As Bitmap
        Private m_bmpWarning As Bitmap

#End Region ' Private vars

#Region " Construction / destruction "

        Public Sub New()
            Me.InitializeComponent()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
            Me.m_bmpError = New Bitmap(ScientificInterfaceShared.My.Resources.Critical, c_imgradius * 2, c_imgradius * 2)
            Me.m_bmpWarning = New Bitmap(ScientificInterfaceShared.My.Resources.Warning, c_imgradius * 2, c_imgradius * 2)
        End Sub

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                    Me.m_bmpError.Dispose()
                    Me.m_bmpError = Nothing
                    Me.m_bmpWarning.Dispose()
                    Me.m_bmpWarning = Nothing
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region ' Construction / destruction

#Region " Properties "

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Public Property UIContext As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(value As ScientificInterfaceShared.Controls.cUIContext)

                ' Clean up
                If (Me.m_uic IsNot Nothing) Then
                    Me.m_uic.Core.Messages.RemoveMessageHandler(Me.m_mhSpace)
                    Me.m_mhSpace = Nothing
                End If

                ' Update
                Me.m_uic = value

                ' Config
                If (Me.m_uic IsNot Nothing) Then
                    Me.m_mhSpace = New cMessageHandler(AddressOf OnCoreMessage, eCoreComponentType.EcoSpace, eMessageType.Any, Me.m_uic.SyncObject)
                    Me.m_uic.Core.Messages.AddMessageHandler(Me.m_mhSpace)
#If DEBUG Then
                    Me.m_mhSpace.Name = "ucSpatialTimeSeriesToolbox::m_mhSpace"
#End If

                End If
            End Set
        End Property

        Public Property Filter As eVarNameFlags
            Get
                Return Me.m_varname
            End Get
            Set(value As eVarNameFlags)
                If (Me.m_varname = value) Then Return
                Me.m_varname = value
                Me.RecalcLayout()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Selection changed notification event
        ''' </summary>
        ''' <param name="owner">The sender of this event</param>
        ''' <param name="ds">The selected datasets</param>
        ''' -------------------------------------------------------------------
        Public Event OnSelectedDatasetChanged(owner As Object, ds As ISpatialDataSet)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the zero-based index of the selected <see cref="ISpatialDataSet"/>. 
        ''' This value cannot be equal to or exceed <see cref="cSpatialDataSetManager.Count"/>
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property SelectedDatasetIndex As Integer
            Get
                Return Me.m_iSelectedIndex
            End Get
            Set(value As Integer)

                Me.m_iSelectedIndex = Math.Min(Me.m_lInfo.Count - 1, Math.Max(-1, value))
                Me.Invalidate()

                If (Me.UIContext Is Nothing) Then Return

                Dim ds As ISpatialDataSet = Nothing
                If (Me.m_iSelectedIndex >= 0) Then ds = Me.m_lInfo(Me.m_iSelectedIndex).m_ds
                Try
                    RaiseEvent OnSelectedDatasetChanged(Me, ds)
                Catch ex As Exception

                End Try
            End Set
        End Property

        Public Event OnSelectedTimestepChanged(owner As Object, iTimeStep As Integer, dt As DateTime)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the one-based index of the selected Ecospace time step. 
        ''' This value cannot exceed <see cref="cCore.nEcospaceTimeSteps"/>.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property SelectedTimeStep As Integer
            Get
                Return Me.m_iSelectedTimeStep
            End Get
            Set(value As Integer)

                If (Me.UIContext Is Nothing) Then Return

                Me.m_iSelectedTimeStep = Math.Max(0, Math.Min(value, Me.m_uic.Core.nEcospaceTimeSteps - 1))
                Me.Invalidate()

                Try
                    RaiseEvent OnSelectedTimestepChanged(Me, Me.m_iSelectedTimeStep, Me.m_uic.Core.EcospaceTimestepToAbsoluteTime(Me.m_iSelectedTimeStep))
                Catch ex As Exception

                End Try
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Entirely refresh the content of the toolbox.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub RefreshContent()
            Me.RecalcSize()
            Me.RecalcLayout()
            Me.Invalidate()
        End Sub

#End Region ' Properties

#Region " Form overrides "

        Protected Overrides Sub OnLoad(e As System.EventArgs)

            MyBase.OnLoad(e)
            If (Me.m_uic Is Nothing) Then Return
            Me.m_manSets = Me.m_uic.Core.SpatialDataConnectionManager.DatasetManager
            Me.RefreshContent()

        End Sub

        Protected Overrides Sub OnResize(e As System.EventArgs)
            MyBase.OnResize(e)
            Me.RecalcSize()
            Me.Invalidate(True)
        End Sub

        Protected Overrides Function IsInputKey(keyData As System.Windows.Forms.Keys) As Boolean
            Select Case keyData
                Case Keys.Right, Keys.Left
                    Return True
            End Select
            Return MyBase.IsInputKey(keyData)
        End Function

        Protected Overrides Sub OnKeyDown(e As System.Windows.Forms.KeyEventArgs)
            Dim iInc As Integer = 0
            Select Case e.KeyCode
                Case Keys.Left
                    iInc = -1
                Case Keys.Right
                    iInc = 1
            End Select

            If ((e.Modifiers And Keys.Control) <> 0) Then
                Dim sStepsPerYear As Single = CSng(Me.m_uic.Core.nEcospaceTimeSteps / Math.Max(1, Me.m_uic.Core.nEcospaceYears))
                iInc = CInt(iInc * sStepsPerYear)
            End If

            If (iInc <> 0) Then
                Me.SelectedTimeStep = Math.Min(Me.m_uic.Core.nEcospaceTimeSteps, Math.Max(1, Me.m_iSelectedTimeStep + iInc))
            End If
            MyBase.OnKeyDown(e)
        End Sub

        Protected Overrides Sub OnMouseDoubleClick(e As System.Windows.Forms.MouseEventArgs)

            'MyBase.OnMouseDoubleClick(e)

            Dim info As cDatasetInfo = Me.DatasetFromPoint(e.Location)
            If (info Is Nothing) Then Return

            Dim cmd As cEditSpatialDatasetCommand = CType(Me.UIContext.CommandHandler.GetCommand(cEditSpatialDatasetCommand.COMMAND_NAME), cEditSpatialDatasetCommand)
            cmd.Invoke(info.m_ds)

        End Sub

        Protected Overrides Sub OnScroll(se As System.Windows.Forms.ScrollEventArgs)
            Me.Invalidate()
            MyBase.OnScroll(se)
        End Sub

        Protected Overrides Sub OnMouseClick(e As System.Windows.Forms.MouseEventArgs)
            Dim ptClick As New Point(e.Location.X - Me.AutoScrollPosition.X, e.Location.Y - Me.AutoScrollPosition.Y)
            Dim pos As cDatasetInfo = Me.DatasetFromPoint(ptClick)
            If (pos IsNot Nothing) Then
                Me.SelectedDatasetIndex = pos.m_iPosVert
            End If
            Me.SelectedTimeStep = TimestepFromPoint(ptClick)
            MyBase.OnMouseClick(e)
        End Sub

        Private m_strTipText As String = ""

        Protected Overrides Sub OnMouseMove(e As System.Windows.Forms.MouseEventArgs)

            Dim ptClick As New Point(e.Location.X - Me.AutoScrollPosition.X, e.Location.Y - Me.AutoScrollPosition.Y)
            Dim pos As cDatasetInfo = Me.DatasetFromPoint(ptClick)
            Dim strText As String = ""

            If (pos IsNot Nothing) Then
                Dim comp As cDatasetCompatilibity = m_manSets.Compatibility(pos.m_ds)
                Dim iStep As Integer = TimestepFromPoint(ptClick)
                Dim dtStep As Date = Me.m_uic.Core.EcospaceTimestepToAbsoluteTime(iStep)
                Dim strDate As String = dtStep.ToShortDateString

                Select Case comp.CompatibilityAt(iStep)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NoTemporal, _
                         cDatasetCompatilibity.eCompatibilityTypes.NotSet
                        strText = pos.m_ds.DisplayName

                    Case cDatasetCompatilibity.eCompatibilityTypes.Errors
                        strText = String.Format(My.Resources.SPATIALTEMP_STATUS_T_MISSING, pos.m_ds.DisplayName, iStep, strDate)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NoSpatial
                        strText = String.Format(My.Resources.SPATIALTEMP_STATUS_T_NOSPATIAL, pos.m_ds.DisplayName, iStep, strDate)

                    Case cDatasetCompatilibity.eCompatibilityTypes.PartialSpatial
                        strText = String.Format(My.Resources.SPATIALTEMP_STATUS_T_PARTIALSPATIAL, pos.m_ds.DisplayName, iStep, strDate)

                    Case cDatasetCompatilibity.eCompatibilityTypes.TotalOverlap
                        strText = String.Format(My.Resources.SPATIALTEMP_STATUS_T_FULLSPATIAL, pos.m_ds.DisplayName, iStep, strDate)

                    Case cDatasetCompatilibity.eCompatibilityTypes.TemporalNotIndexed
                        strText = String.Format(My.Resources.SPATIALTEMP_STATUS_T_UNKNOWN, pos.m_ds.DisplayName, iStep, strDate)

                End Select
            End If

            ' Async update to prevent flickering
            If (strText <> Me.m_strTipText) Then
                Me.m_strTipText = strText
                BeginInvoke(New MethodInvoker(AddressOf UpdateTooltip))
            End If

        End Sub

        Private Sub UpdateTooltip()
            cToolTipShared.GetInstance().SetToolTip(Me, Me.m_strTipText)
        End Sub

        Protected Overrides Sub OnPaint(e As System.Windows.Forms.PaintEventArgs)

            MyBase.OnPaint(e)

            If (Me.m_uic Is Nothing) Then Return

            e.Graphics.Clear(Me.BackColor)
            Try

                ' Paint matrix shifted to X and Y scroll position
                e.Graphics.Transform = New Matrix(1, 0, 0, 1, AutoScrollPosition.X, AutoScrollPosition.Y)
                For i As Integer = 0 To Me.m_lInfo.Count - 1
                    Me.DrawDatasetIndicator(e.Graphics, Me.m_lInfo(i), i = Me.m_iSelectedIndex)
                Next
                Me.DrawGrid(e.Graphics, New Rectangle(0, c_headerheight, Me.m_iTimestepSize * Me.m_uic.Core.nEcospaceTimeSteps, Me.ClientRectangle.Height - c_headerheight))
                For i As Integer = 0 To Me.m_lInfo.Count - 1
                    Me.DrawDataset(e.Graphics, Me.m_lInfo(i), i = Me.m_iSelectedIndex)
                Next
                e.Graphics.ResetTransform()

                ' Paint header at the top of the visible scroll area
                e.Graphics.Transform = New Matrix(1, 0, 0, 1, AutoScrollPosition.X, 0)
                Me.DrawGridHeader(e.Graphics, New Rectangle(0, 0, Me.m_iTimestepSize * Me.m_uic.Core.nEcospaceTimeSteps, c_headerheight))
                e.Graphics.ResetTransform()

            Catch ex As Exception
                Debug.Assert(False)
            End Try

        End Sub

#End Region ' Form overrides

#Region " Events "

        Private Sub OnCoreMessage(ByRef msg As cMessage)

            If (msg.Source <> eCoreComponentType.EcoSpace) Then Return

            Select Case msg.DataType
                Case eDataTypes.EcospaceSpatialDataConnection
                    If (msg.Type = eMessageType.DataModified) Then
                        Me.Invalidate()
                    ElseIf (msg.Type = eMessageType.Progress) Then
                        Me.Invalidate()
                    ElseIf (msg.Type = eMessageType.DataAddedOrRemoved) Then
                        Me.RefreshContent()
                    End If
                Case eDataTypes.EcoSpaceScenario, eDataTypes.EcospaceModelParameter
                    Me.RefreshContent()
                Case eDataTypes.EcospaceSpatialDataSource
                    Me.RefreshContent()
            End Select

        End Sub

#End Region ' Events

#Region " Internals "

        Protected Sub RecalcSize()

            ' Safety check
            If (Me.m_uic Is Nothing) Then Return
            ' Calc number of pixels per time step
            Me.m_iTimestepSize = CInt(Math.Max(4, Math.Floor(Me.Width / Me.m_uic.Core.nEcospaceTimeSteps)))

            Me.AutoScroll = True
            Me.AutoScrollMinSize = New Size(Me.m_iTimestepSize * Me.m_uic.Core.nEcospaceTimeSteps, (Me.m_lInfo.Count * (c_barheight + 2 * c_barmargin) + c_headerheight))
            Me.AutoScrollMargin = New Size(0, 0)

        End Sub

        ''' <summary>
        ''' Calculate dataset display rectangles
        ''' </summary>
        Protected Sub RecalcLayout()

            ' Safety check
            If (Me.m_uic Is Nothing) Then Return

            Dim core As cCore = Me.m_uic.Core
            Dim bm As cEcospaceBasemap = core.EcospaceBasemap
            Dim man As cSpatialDataConnectionManager = Me.m_uic.Core.SpatialDataConnectionManager()
            Dim lAdt As New List(Of cSpatialDataAdapter)
            Dim iRow As Integer = 0
            Dim ptfTL As PointF = bm.PosTopLeft
            Dim ptfBR As PointF = bm.PosBottomRight

            ' Resolve varname
            If (Me.m_varname = eVarNameFlags.NotSet) Then
                lAdt.AddRange(man.Adapters)
            Else
                lAdt.Add(man.Adapter(Me.m_varname))
            End If

            ' Try to preserve selection
            Dim var As eVarNameFlags = eVarNameFlags.NotSet
            Dim guid As Guid
            Dim iSel As Integer = 0

            If (Me.m_iSelectedIndex > 0) Then
                var = Me.m_lInfo(Me.m_iSelectedIndex).m_var
                guid = Me.m_lInfo(Me.m_iSelectedIndex).m_guid
            End If

            Me.m_lInfo.Clear()

            Dim dicConn As New Dictionary(Of ISpatialDataSet, cSpatialDataConnection)
            For Each adt As cSpatialDataAdapter In lAdt
                For Each conn As cSpatialDataConnection In adt.Connections()
                    Debug.Assert(conn IsNot Nothing)
                    If (adt.IsEnabled(conn.iLayer)) Then
                        dicConn(conn.Dataset) = conn
                    End If
                Next conn
            Next

            For Each conn As cSpatialDataConnection In dicConn.Values

                Dim pos As New cDatasetInfo()
                pos.m_ds = conn.Dataset
                pos.m_var = conn.Adapter.VarName
                pos.m_guid = conn.Dataset.GUID
                pos.m_iPosVert = iRow

                If conn.Dataset.TimeStart = Date.MinValue Then
                    pos.m_iTimeStart = 1
                Else
                    pos.m_iTimeStart = core.AbsoluteTimeToEcospaceTimestep(conn.Dataset.TimeStart)
                End If

                If conn.Dataset.TimeEnd = Date.MaxValue Then
                    pos.m_iTimeEnd = core.nEcospaceTimeSteps
                Else
                    pos.m_iTimeEnd = Me.m_uic.Core.AbsoluteTimeToEcospaceTimestep(conn.Dataset.TimeEnd)
                End If

                For iStep As Integer = pos.m_iTimeStart To pos.m_iTimeEnd
                    Dim tm As DateTime = core.EcospaceTimestepToAbsoluteTime(iStep)
                    If conn.Dataset.HasDataAtT(tm) Then
                        pos.m_liData.Add(iStep)
                        pos.m_liTime.Add(tm)
                    End If
                Next

                Me.m_lInfo.Add(pos)

                If (pos.m_var = var) And (pos.m_guid = guid) Then
                    iSel = iRow
                End If

                iRow += 1
            Next conn

            Me.SelectedDatasetIndex = iSel

        End Sub

        ''' <summary>
        ''' Paint the header row
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        Private Sub DrawGridHeader(g As Graphics, rc As Rectangle)

            g.FillRectangle(SystemBrushes.Control, rc)

            Dim iYear As Integer = Me.m_uic.Core.EcosimFirstYear
            Dim core As cCore = Me.m_uic.Core
            Dim sStepsPerYear As Single = CSng(Me.m_uic.Core.nEcospaceTimeSteps / Math.Max(1, Me.m_uic.Core.nEcospaceYears))

            Using ft As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Scale)
                For i As Integer = 0 To Me.m_uic.Core.nEcospaceYears Step 5
                    Dim sx As Single = i * sStepsPerYear * Me.m_iTimestepSize
                    g.DrawString(CStr(iYear + i), ft, SystemBrushes.ControlText, sx, 0.0!)
                    Using p As New Pen(SystemColors.ControlDarkDark, 1)
                        p.DashStyle = DashStyle.Dot
                        g.DrawLine(p, rc.X + sx, rc.Y, rc.X + sx, rc.Y + rc.Height)
                    End Using
                Next
            End Using

        End Sub

        ''' <summary>
        ''' Draw a grid of vertical lines for every 5 years
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="rc"></param>
        Private Sub DrawGrid(g As Graphics, rc As Rectangle)

            Dim iYear As Integer = Me.m_uic.Core.EcosimFirstYear
            Dim core As cCore = Me.m_uic.Core
            Dim sStepsPerYear As Single = CSng(Me.m_uic.Core.nEcospaceTimeSteps / Math.Max(1, Me.m_uic.Core.nEcospaceYears))

            Using p As New Pen(SystemColors.ControlDarkDark, 1)
                p.DashStyle = DashStyle.Dot
                For i As Integer = 0 To Me.m_uic.Core.nEcospaceYears
                    Dim sx As Single = i * sStepsPerYear * Me.m_iTimestepSize
                    g.DrawLine(p, rc.X + sx, rc.Y, sx, rc.Y + rc.Height)
                Next
            End Using

            Using p As New Pen(Me.m_uic.StyleGuide.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT), 2)
                Dim sx As Single = Me.m_iSelectedTimeStep * Me.m_iTimestepSize
                g.DrawLine(p, rc.X + sx, rc.Y, sx, rc.Y + rc.Height)
            End Using

        End Sub

        ''' <summary>
        ''' Draw a shaded area to indicate the presence of a dataset. This area falls below the time step grid
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pos"></param>
        Private Sub DrawDatasetIndicator(ByVal g As Graphics, _
                                         ByVal pos As cDatasetInfo, _
                                         ByVal bSelected As Boolean)

            Dim rcBar As Rectangle = Me.DatasetArea(pos)
            Dim rcBack As Rectangle = New Rectangle(-Me.AutoScrollPosition.X, rcBar.Y - c_barmargin, Me.ClientRectangle.Width, rcBar.Height + 2 * c_barmargin)
            Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(pos.m_ds)

            ' Fill back bar
            Using br As New SolidBrush(cColorUtils.GetVariant(cStyleGuide.GetColor(comp), 0.75))
                g.FillRectangle(br, rcBack)
            End Using
            g.DrawLine(Pens.White, rcBack.X, rcBack.Y, rcBack.X + rcBack.Width, rcBack.Y)

        End Sub


        ''' <summary>
        ''' Draw the actual dataset bar, data points and labels
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pos"></param>
        Private Sub DrawDataset(ByVal g As Graphics, _
                                ByVal pos As cDatasetInfo, _
                                ByVal bSelected As Boolean)

            Dim rcBar As Rectangle = Me.DatasetArea(pos)
            Dim rcBack As Rectangle = New Rectangle(-Me.AutoScrollPosition.X, rcBar.Y - c_barmargin, Me.ClientRectangle.Width, rcBar.Height + 2 * c_barmargin)
            Dim rcLabel As New Rectangle(rcBar.X, rcBar.Y, rcBar.Width, c_barlabelheight)
            Dim rcDot As New Rectangle(rcBar.X, rcBar.Y + c_barheight - CInt((c_barheight - c_barlabelheight) / 2) - c_dotradius, 2 * c_dotradius, 2 * c_dotradius)
            Dim rcImg As New Rectangle(rcBar.X, CInt(rcBar.Y + c_barheight - CInt((c_barheight - c_barlabelheight) / 2) - c_imgradius), 2 * c_imgradius, 2 * c_imgradius)

            Dim comp As cDatasetCompatilibity = Me.m_manSets.Compatibility(pos.m_ds)
            Dim clrBar As Color = cStyleGuide.GetColor(comp)
            Dim clrText As Color = SystemColors.ControlText
            Dim clrOutline As Color
            Dim iWidthOutline As Integer = 1

            ' Is off-screen?
            Dim bOutRight As Boolean = (rcBar.X > AutoScrollPosition.X + Me.ClientRectangle.Width)
            Dim bOutLeft As Boolean = ((rcBar.X + rcBar.Width) < AutoScrollPosition.X)

            If bSelected Then
                clrOutline = SystemColors.Highlight
                iWidthOutline = 2
            Else
                clrOutline = cColorUtils.GetVariant(clrBar, -0.5)
            End If

            ' Fill area bar
            Using br As New SolidBrush(clrBar)
                g.FillRectangle(br, rcBar)
            End Using
            ' Draw outline
            Using p As New Pen(clrOutline, iWidthOutline)
                g.DrawRectangle(p, rcBar)
            End Using

            ' Draw text within bar area, but as much on-screen as possible
            Using ft As Font = Me.m_uic.StyleGuide.Font(cStyleGuide.eApplicationFontType.Scale)
                rcLabel.Width = rcBack.Width
                g.DrawString(pos.m_ds.DisplayName, ft, SystemBrushes.ControlText, Math.Max(rcBack.X, rcLabel.X), rcLabel.Y)
            End Using

            For i As Integer = 0 To pos.m_liData.Count - 1
                Dim iStep As Integer = pos.m_liData(i)
                rcDot.X = rcBar.X + (iStep - pos.m_iTimeStart) * Me.m_iTimestepSize - c_dotradius
                rcImg.X = rcBar.X + (iStep - pos.m_iTimeStart) * Me.m_iTimestepSize - c_imgradius

                Select Case comp.CompatibilityAt(iStep)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NotSet, _
                         cDatasetCompatilibity.eCompatibilityTypes.TemporalNotIndexed
                        g.FillEllipse(Brushes.White, rcDot)
                        g.DrawEllipse(Pens.Black, rcDot)

                    Case cDatasetCompatilibity.eCompatibilityTypes.Errors
                        g.DrawImage(Me.m_bmpError, rcImg.Location)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NoSpatial
                        g.DrawImage(Me.m_bmpWarning, rcImg.Location)

                    Case cDatasetCompatilibity.eCompatibilityTypes.NoTemporal
                        'NOP

                    Case cDatasetCompatilibity.eCompatibilityTypes.PartialSpatial
                        g.DrawImage(Me.m_bmpWarning, rcImg.Location)

                    Case cDatasetCompatilibity.eCompatibilityTypes.TotalOverlap
                        g.FillEllipse(Brushes.Green, rcDot)
                        g.DrawEllipse(Pens.Black, rcDot)

                End Select

            Next

        End Sub

        Private Function DatasetPos(ds As ISpatialDataSet) As cDatasetInfo
            If ds Is Nothing Then Return Nothing
            For Each pos As cDatasetInfo In Me.m_lInfo
                If ReferenceEquals(pos.m_ds, ds) Then Return pos
            Next
            Return Nothing
        End Function

        Private Function DatasetArea(pos As cDatasetInfo) As Rectangle
            Dim iStart As Integer = pos.m_iTimeStart * Me.m_iTimestepSize
            Dim iEnd As Integer = (pos.m_iTimeEnd + 1) * Me.m_iTimestepSize - 1
            Return New Rectangle(iStart, c_headerheight + pos.m_iPosVert * (c_barheight + 2 * c_barmargin) + c_barmargin, iEnd - iStart, c_barheight)
        End Function

        Private Function TimestepFromPoint(pt As Point) As Integer
            If (Me.m_iTimestepSize = 0) Then Return -1
            Return CInt(Math.Round(pt.X / Me.m_iTimestepSize))
        End Function

        Private Function DatasetFromPoint(pt As Point) As cDatasetInfo
            If (pt.Y < c_headerheight) Then Return Nothing
            For Each pos As cDatasetInfo In Me.m_lInfo
                ' JS 30Nov14: only test Y-coordinate fit
                'If Me.DatasetArea(pos).Contains(pt) Then Return pos
                Dim area As Rectangle = Me.DatasetArea(pos)
                If ((pt.Y >= area.Y) And (pt.Y <= (area.Y + area.Height))) Then Return pos
            Next
            Return Nothing
        End Function

#End Region ' Internals

    End Class

End Namespace ' Ecospace.Controls