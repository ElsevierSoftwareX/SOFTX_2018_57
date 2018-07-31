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
Imports System.ComponentModel
Imports System.Text
Imports EwECore.Style
Imports EwEUtils.Core
Imports EwEUtils.SystemUtilities
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Controls.Map
Imports ScientificInterfaceShared.Definitions
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper class, implements the actual display of an EwE flow diagram using
    ''' a simple layout of nodes connected via arched lines.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cTreeFlowDiagramRenderer
        Implements IFlowDiagramRenderer

#Region " Helper classes "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cFlowDiagramNode

            '''--------------------------------------------------------------------
            ''' <summary>
            ''' Draw a flow diagram node.
            ''' </summary>
            ''' <param name="g">Graphics to render the node onto.</param>
            ''' <param name="ptf">Point of the node center.</param>
            ''' <param name="nodetype"><see cref="eFDNodeTypes">Node render type</see>.</param>
            ''' <param name="iSize">Node render size, in pixels.</param>
            ''' <param name="clrLine">Node line colour.</param>
            ''' <param name="clrFill">Node fill colour.</param>
            '''--------------------------------------------------------------------
            Public Sub DrawNode(ByVal g As Graphics, _
                                ByVal ptf As PointF, _
                                ByVal nodetype As eFDNodeTypes, _
                                ByVal iSize As Integer, _
                                ByVal clrLine As Color, _
                                ByVal clrFill As Color)

                Using br As New SolidBrush(clrFill)
                    Using p As New Pen(clrLine)
                        Select Case nodetype
                            Case eFDNodeTypes.Circle
                                g.FillEllipse(br, ptf.X - CInt(iSize / 2), ptf.Y - CInt(iSize / 2), iSize, iSize)
                                g.DrawEllipse(p, ptf.X - CInt(iSize / 2), ptf.Y - CInt(iSize / 2), iSize, iSize)
                            Case eFDNodeTypes.Rectangle
                                g.FillRectangle(br, ptf.X - CInt(iSize / 2), ptf.Y - CInt(iSize / 2), iSize, iSize)
                                g.DrawRectangle(p, ptf.X - CInt(iSize / 2), ptf.Y - CInt(iSize / 2), iSize, iSize)
                            Case Else
                                Debug.Assert(False)
                        End Select
                    End Using
                End Using

            End Sub

            '''--------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="g"></param>
            ''' <param name="ptf"></param>
            ''' <param name="font"></param>
            ''' <param name="clrFont"></param>
            ''' <param name="strText">Formatted label text to draw.</param>
            '''--------------------------------------------------------------------
            Public Sub DrawLabel(ByVal g As Graphics, _
                                 ByVal ptf As PointF, _
                                 ByVal font As Font, _
                                 ByVal clrFont As Color, _
                                 ByVal strText As String)

                Using br As New SolidBrush(clrFont)
                    g.DrawString(strText, font, br, ptf, cTreeFlowDiagramRenderer.g_fmt)
                End Using

            End Sub

            Friend Function CalcLabelSize(ByVal g As Graphics, _
                                          ByVal font As Font, _
                                          ByVal strText As String, _
                                          ByVal fmt As StringFormat) As SizeF
                Return g.MeasureString(strText, font, 6000, fmt)
            End Function

        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cFlowDiagramConnector

#Region " Rendering "

            Public Sub DrawConnection(ByVal g As Graphics, _
                                      ByVal ptFrom As PointF, _
                                      ByVal ptTo As PointF, _
                                      ByVal clrLine As Color, _
                                      ByVal sWidth As Single, _
                                      ByVal connectiontype As eFDConnectionType)

                Dim pn As New Pen(clrLine, sWidth)

                Select Case connectiontype

                    Case eFDConnectionType.Straight
                        g.DrawLine(pn, ptFrom.X, ptFrom.Y, ptTo.X, ptTo.Y)

                    Case eFDConnectionType.Arch
                        ' Test if on top of each other
                        ' ToDo: use range comparison here
                        If (ptFrom.X <> ptTo.X) And (ptFrom.Y <> ptTo.Y) Then
                            Me.DrawArc(g, pn, ptFrom, ptTo)
                        Else
                            g.DrawLine(pn, ptFrom.X, ptFrom.Y, ptTo.X, ptTo.Y)
                        End If
                End Select

                pn.Dispose()

            End Sub

            Private Sub DrawArc(ByVal g As Graphics, ByVal pn As Pen, ByVal location1 As PointF, ByVal location2 As PointF)

                Dim sAngleSweep As Single = 90.0!
                Dim sAngleStart As Single = 0.0!
                Dim rcArc As RectangleF = New RectangleF(0, 0, 1, 1)
                Dim szArc As SizeF = New SizeF(Math.Abs(location1.X - location2.X) * 2, Math.Abs(location1.Y - location2.Y) * 2)

                If location1.X > location2.X And location1.Y > location2.Y Then
                    rcArc = New RectangleF(New PointF(location2.X, location2.Y - szArc.Height / 2), szArc)
                    sAngleStart = 180.0!
                    sAngleSweep = -90.0!
                ElseIf location1.X > location2.X And location1.Y < location2.Y Then
                    rcArc = New RectangleF(New PointF(location2.X, location1.Y), szArc)
                    sAngleStart = 180.0!
                    sAngleSweep = 90.0!
                ElseIf location1.X < location2.X And location1.Y > location2.Y Then
                    rcArc = New RectangleF(New PointF(location1.X - szArc.Width / 2, location2.Y - szArc.Height / 2), szArc)
                    sAngleStart = 0.0!
                    sAngleSweep = 90.0!
                ElseIf location1.X < location2.X And location1.Y < location2.Y Then
                    rcArc = New RectangleF(New PointF(location1.X - szArc.Width / 2, location1.Y), szArc)
                    sAngleStart = 0.0!
                    sAngleSweep = -90.0!
                End If

                g.DrawArc(pn, rcArc, sAngleStart, sAngleSweep)

            End Sub

#End Region ' Rendering

        End Class

#End Region ' Helper classes

#Region " Privates "

        Private m_data As IFlowDiagramData = Nothing

        Private m_colorramp As New cEwEColorRamp()
        Private m_iNumTrophicLevels As Integer = 6
        Private m_bShowTrophicLevels As Boolean = True
        Private m_sAngle() As Single            '' To store where the angle is relative to 0
        Private m_asLabelOffsetX() As Single
        Private m_asLabelOffsetY() As Single
        Private m_node As cFlowDiagramNode = Nothing
        Private m_connectors As cFlowDiagramConnector = Nothing
        Private m_clrNode As Color = Color.LightGray
        Private m_bAutoNodeSize As Boolean = True
        Private m_iNodeSize As Integer = 10
        Private m_bIsDrawLabel As Boolean = True
        Private m_bIsNodeDrawValue As Boolean = False
        Private m_clrLine As Color = Color.Gray
        Private m_bAutoLineWidth As Boolean = False
        Private m_bShowTitle As Boolean = True
        Private m_sLineWidth As Single = 0.5
        Private m_nodetype As eFDNodeTypes = eFDNodeTypes.Circle
        Private m_connectiontype As eFDConnectionType = eFDConnectionType.Arch
        Private m_colorusagetype As eFDColorUsageTypes = eFDColorUsageTypes.None
        Private m_tsShowLegend As TriState = TriState.UseDefault
        Private m_nodeshowtype As eFDShowHiddenType = eFDShowHiddenType.GrayedOut
        Private m_nodescaletype As eFDNodeScaleType = eFDNodeScaleType.Logarithmic

        Private Shared g_fmt As New StringFormat()
        ''' <summary>Minimum mouse hit area size</summary>
        Private Shared g_minsize As Integer = 10

        Private m_bInUpdate As Boolean = False

#End Region ' Privates

        Public Event OnChanged(ByVal sender As cTreeFlowDiagramRenderer)

#Region " Constructor "

        Public Sub New(ByVal data As IFlowDiagramData)

            Debug.Assert(data IsNot Nothing)

            Me.m_data = data

            ReDim Me.m_sAngle(Me.m_data.NumItems)
            ReDim Me.m_asLabelOffsetX(Me.m_data.NumItems)
            ReDim Me.m_asLabelOffsetY(Me.m_data.NumItems)

            Me.m_node = New cFlowDiagramNode()
            Me.m_connectors = New cFlowDiagramConnector()
            ' Elminate near-white colours
            Me.m_colorramp.ColorOffsetStart = 0.2!

            cTreeFlowDiagramRenderer.g_fmt.Alignment = StringAlignment.Center
            Me.InitNodePositions()

        End Sub

#End Region ' Constructor

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IUIElement.UIContext"/>
        ''' -------------------------------------------------------------------
        Private ReadOnly Property UIContext() As cUIContext
            Get
                Return DirectCast(Me.m_data, IUIElement).UIContext
            End Get
        End Property

#Region " Drawing "

        Friend Sub DrawBackground(ByVal g As Graphics, ByVal rc As Rectangle) _
            Implements IFlowDiagramRenderer.DrawBackground

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Dim iUnitHeight As Integer = CInt(rc.Height / Me.m_iNumTrophicLevels)

            Using brBack As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.IMAGE_BACKGROUND))
                g.FillRectangle(brBack, rc)
            End Using

            Using brText As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT))
                Using font As Font = sg.Font(cStyleGuide.eApplicationFontType.Scale)
                    For i As Integer = 1 To m_iNumTrophicLevels - 1
                        If (Me.m_bShowTrophicLevels) Then
                            g.DrawString((m_iNumTrophicLevels - i).ToString, font, brText, 20, i * iUnitHeight)
                        End If
                        g.DrawLine(Pens.LightGray, 20, i * iUnitHeight, rc.Width - 20, i * iUnitHeight)
                    Next i
                End Using
            End Using

        End Sub

        Friend Sub DrawTitle(ByVal g As Graphics, ByVal rc As Rectangle) _
            Implements IFlowDiagramRenderer.DrawTitle

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Dim strTitle As String = Me.m_data.Title

            If (Not Me.m_bShowTitle) Or (String.IsNullOrWhiteSpace(strTitle)) Then Return

            Using brText As New SolidBrush(sg.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT))
                Using font As Font = sg.Font(cStyleGuide.eApplicationFontType.Title)
                    Dim szf As SizeF = g.MeasureString(strTitle, font)
                    g.DrawString(strTitle, font, brText, rc.X + (rc.Width - szf.Width) / 2, rc.Y + font.Height * 3)
                End Using
            End Using

        End Sub

        Friend Sub DrawNode(ByVal g As Graphics,
                            ByVal rc As Rectangle,
                            ByVal iGroup As Integer,
                            ByVal highlight As IFlowDiagramRenderer.eFDHighlightType) _
            Implements IFlowDiagramRenderer.DrawNode

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Dim strLabel As String = Me.FormatLabelText(iGroup)
            Dim sValue As Single = Me.m_data.Value(iGroup)
            Dim sValueMax As Single = Me.m_data.ValueMax
            Dim clrPen As Color = Me.TextColor()
            Dim clrLabel As Color = clrPen

            Dim clrFill As Color = Color.LightGray
            Dim iSize As Integer = Me.CalcNodeSize(sValue, sValueMax)

            Select Case highlight

                Case IFlowDiagramRenderer.eFDHighlightType.Invisible
                    Return

                Case IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                    clrPen = Color.LightGray
                    clrFill = Color.White
                    clrLabel = cColorUtils.GetVariant(clrLabel, 0.5!)

                Case IFlowDiagramRenderer.eFDHighlightType.None
                    Select Case Me.m_colorusagetype
                        Case eFDColorUsageTypes.EwE
                            clrFill = Me.m_data.ItemColor(iGroup)
                        Case eFDColorUsageTypes.Value
                            clrFill = Me.m_colorramp.GetColor(sValue, sValueMax)
                        Case eFDColorUsageTypes.TrophicLevel
                            clrFill = Me.m_colorramp.GetColor(Me.m_data.TrophicLevel(iGroup) - 1, Me.m_iNumTrophicLevels - 1)
                        Case Else
                            clrFill = Me.m_clrNode
                    End Select

                Case IFlowDiagramRenderer.eFDHighlightType.Selected
                    clrFill = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)

                Case IFlowDiagramRenderer.eFDHighlightType.LinkIn
                    clrPen = Me.InLinkColor

                Case IFlowDiagramRenderer.eFDHighlightType.LinkOut
                    clrPen = Me.OutLinkColor

            End Select

            If (sValue = 0) Then
                clrFill = sg.ApplicationColor(cStyleGuide.eApplicationColorType.GENERICERROR_BACKGROUND)
                clrLabel = clrPen
            End If

            Me.m_node.DrawNode(g, Me.NodeLocation(iGroup, rc), Me.NodeType, iSize, clrPen, clrFill)

            If (Me.m_bIsDrawLabel) Then
                Me.m_node.DrawLabel(g, Me.LabelLocation(iGroup, rc), Me.RenderFont, clrLabel, Me.FormatLabelText(iGroup))
            End If

        End Sub

        Friend Sub DrawConnection(ByVal g As Graphics,
                                  ByVal rc As Rectangle,
                                  ByVal iPred As Integer,
                                  ByVal iPrey As Integer,
                                  ByVal highlight As IFlowDiagramRenderer.eFDHighlightType) _
            Implements IFlowDiagramRenderer.DrawConnection

            Dim clrLine As Color = Me.m_clrLine
            Dim sDiet As Single = Me.m_data.LinkValue(iPred, iPrey)
            Dim sDietMax As Single = Me.m_data.LinkValueMax
            Dim sLineWidth As Single = 0.5!

            If sDiet <= 0 Then Return

            Select Case highlight

                Case IFlowDiagramRenderer.eFDHighlightType.None
                    Select Case Me.m_colorusagetype
                        Case eFDColorUsageTypes.Flow
                            clrLine = Me.m_colorramp.GetColor(sDiet, sDietMax)
                        Case Else
                            ' Normal
                    End Select

                Case IFlowDiagramRenderer.eFDHighlightType.GrayedOut
                    clrLine = cColorUtils.GetVariant(clrLine, 0.75)

                Case IFlowDiagramRenderer.eFDHighlightType.LinkIn
                    clrLine = Me.InLinkColor
                    sLineWidth = Me.CustomLineWidth * sLineWidth

                Case IFlowDiagramRenderer.eFDHighlightType.LinkOut
                    clrLine = Me.OutLinkColor
                    sLineWidth = Me.CustomLineWidth * sLineWidth

                Case IFlowDiagramRenderer.eFDHighlightType.Invisible
                    Return

            End Select

            sLineWidth *= Me.CalcLineWidth(sDiet, sDietMax)

            Me.m_connectors.DrawConnection(g,
                                        Me.NodeLocation(iPred, rc),
                                        Me.NodeLocation(iPrey, rc),
                                        clrLine,
                                        sLineWidth,
                                        Me.LineConnectionType)
        End Sub

        Friend Sub DrawLegend(ByVal g As Graphics, ByVal ptTopLeft As Point) _
            Implements IFlowDiagramRenderer.DrawLegend

            Dim tsShowLegend As TriState = Me.ShowLegend
            If (tsShowLegend = TriState.UseDefault) Then
                Select Case Me.m_colorusagetype
                    Case eFDColorUsageTypes.Value,
                         eFDColorUsageTypes.Flow,
                         eFDColorUsageTypes.TrophicLevel
                        tsShowLegend = TriState.True
                End Select
            End If

            If (tsShowLegend = TriState.True) Then
                Dim strTitle As String = ""
                Dim sMin As Single = 0
                Dim sMax As Single = 0
                Select Case Me.AutoColorUsage
                    Case eFDColorUsageTypes.None
                        Return
                    Case eFDColorUsageTypes.Value
                        sMin = Me.m_data.ValueMin
                        sMax = Me.m_data.ValueMax
                        strTitle = Me.m_data.DataTitle
                    Case eFDColorUsageTypes.EwE
                        sMin = 1
                        sMax = Me.m_data.NumItems
                        strTitle = My.Resources.HEADER_COMPARTMENT
                    Case eFDColorUsageTypes.Flow
                        sMin = Me.m_data.LinkValueMin
                        sMax = Me.m_data.LinkValueMax
                        strTitle = My.Resources.HEADER_LINK
                End Select
                Dim lgd As New cLegend(Me.UIContext, strTitle)
                lgd.AddGradient("", sMin, sMax)
                lgd.Draw(g, ptTopLeft)
            End If

        End Sub

#End Region ' Drawing

#Region " SetPosition "

        Public Sub MoveNode(ByVal rc As Rectangle, ByVal ptNew As PointF, ByVal iNode As Integer) _
            Implements IFlowDiagramRenderer.MoveNode

            Me.NodeLocation(iNode, rc) = ptNew

        End Sub

        Public Sub MoveLabel(ByVal rc As Rectangle, ByVal ptNew As PointF, ByVal iNode As Integer) _
            Implements IFlowDiagramRenderer.MoveLabel

            Me.LabelLocation(iNode, rc) = ptNew

        End Sub

        Friend Sub InitNodePositions()

            Dim iNumTL As Integer = 4
            Dim aiGroupCount(iNumTL) As Integer
            Dim aiGroup(iNumTL) As Integer
            Dim iTL As Integer

            ' Calc how the groups are distributed over trophic levels [1, iNumTL+]
            For iGroup As Integer = 1 To Me.m_data.NumItems
                iTL = iNumTL
                While (Me.m_data.TrophicLevel(iGroup) < iTL) And (iTL > 1)
                    iTL -= 1
                End While
                aiGroupCount(iTL) += 1
            Next

            ' Distribute groups horizontally
            For iGroup As Integer = 1 To Me.m_data.NumItems

                iTL = iNumTL
                While (Me.m_data.TrophicLevel(iGroup) < iTL) And (iTL > 1)
                    iTL -= 1
                End While

                Me.m_sAngle(iGroup) = 360.0! * (aiGroup(iTL) + 0.5!) / aiGroupCount(iTL)
                Me.m_asLabelOffsetX(iGroup) = 0
                Me.m_asLabelOffsetY(iGroup) = 0

                aiGroup(iTL) += 1

            Next

        End Sub

#End Region ' SetPosition

#Region " Configuration "

        <Browsable(True),
            cLocalizedCategory("HEADER_APPEARANCE"),
            cLocalizedDisplayName("LABEL_TITLE"),
            DefaultValue("Flow diagram")>
        Public Property Title() As String
            Get
                Return Me.m_data.Title
            End Get
            Set(ByVal value As String)
                If (value <> Me.m_data.Title) Then
                    Me.m_data.Title = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_APPEARANCE"),
            cLocalizedDisplayName("PROMPT_SHOWTITLE"),
            cLocalizedDescription("PROMPT_SHOWTITLE_DESCR"),
            DefaultValue(True)>
        Public Property ShowTitle() As Boolean
            Get
                Return Me.m_bShowTitle
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bShowTitle) Then
                    Me.m_bShowTitle = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
        cLocalizedCategory("HEADER_APPEARANCE"),
        cLocalizedDisplayName("GENERIC_SHOW_TROPHIC_LEVELS"),
        DefaultValue(5)>
        Public Property ShowTrophicLevels() As Boolean
            Get
                Return Me.m_bShowTrophicLevels
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bShowTrophicLevels) Then
                    Me.m_bShowTrophicLevels = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_APPEARANCE"),
            cLocalizedDisplayName("GENERIC_SHOW_NUMTL"),
            DefaultValue(5)>
        Public Property NumberOfTrophicLevels() As Integer
            Get
                Return Me.m_iNumTrophicLevels - 1
            End Get
            Set(ByVal value As Integer)
                If ((value + 1) <> Me.m_iNumTrophicLevels) Then
                    Me.m_iNumTrophicLevels = value + 1
                    Me.Update()
                    Me.InitNodePositions()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_APPEARANCE"),
            cLocalizedDisplayName("GENERIC_COLOUR_USAGE"),
            DefaultValue(eFDColorUsageTypes.None)>
        Public Property AutoColorUsage() As eFDColorUsageTypes
            Get
                Return Me.m_colorusagetype
            End Get
            Set(ByVal value As eFDColorUsageTypes)
                If (value <> Me.m_colorusagetype) Then
                    Me.m_colorusagetype = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_APPEARANCE"),
            cLocalizedDisplayName("GENERIC_SHOW_LEGEND"),
            DefaultValue(TriState.UseDefault)>
        Public Property ShowLegend As TriState
            Get
                Return Me.m_tsShowLegend
            End Get
            Set(ByVal value As TriState)
                If (value <> Me.m_tsShowLegend) Then
                    Me.m_tsShowLegend = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
             cLocalizedCategory("HEADER_COMPARTMENT"),
             cLocalizedDisplayName("HEADER_COLOR"),
             DefaultValue(&HFFD3D3D3)>
        Public Property CustomNodeColor() As Color
            Get
                Return Me.m_clrNode
            End Get
            Set(ByVal value As Color)
                If (value <> Me.m_clrNode) Then
                    Me.m_clrNode = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_NODE_SIZE_AUTO"),
            DefaultValue(True)>
        Public Property AutoNodeSize() As Boolean
            Get
                Return Me.m_bAutoNodeSize
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bAutoNodeSize) Then
                    Me.m_bAutoNodeSize = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_NODE_SIZE"),
            DefaultValue(True)>
        Public Property CustomNodeSize() As Integer
            Get
                Return Me.m_iNodeSize
            End Get
            Set(ByVal value As Integer)
                If (value <> Me.m_iNodeSize) Then
                    Me.m_iNodeSize = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_NODE_SCALE"),
            DefaultValue(eFDNodeScaleType.Logarithmic)>
        Public Property NodeScaleType() As eFDNodeScaleType
            Get
                Return Me.m_nodescaletype
            End Get
            Set(ByVal value As eFDNodeScaleType)
                If (value <> Me.m_nodescaletype) Then
                    Me.m_nodescaletype = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_NODE_TYPE"),
            DefaultValue(eFDNodeTypes.Circle)>
        Public Property NodeType() As eFDNodeTypes
            Get
                Return Me.m_nodetype
            End Get
            Set(ByVal value As eFDNodeTypes)
                If (value <> Me.m_nodetype) Then
                    Me.m_nodetype = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_LINK"),
            cLocalizedDisplayName("HEADER_LINE_WIDTH_AUTO"),
            DefaultValue(False)>
        Public Property AutoLineWidth() As Boolean
            Get
                Return Me.m_bAutoLineWidth
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bAutoLineWidth) Then
                    Me.m_bAutoLineWidth = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_LINK"),
            cLocalizedDisplayName("HEADER_LINE_WIDTH"),
            DefaultValue(1)>
        Public Property CustomLineWidth() As Single
            Get
                Return Me.m_sLineWidth
            End Get
            Set(ByVal value As Single)
                If (value <> Me.m_sLineWidth) Then
                    Me.m_sLineWidth = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_LINK"),
            cLocalizedDisplayName("HEADER_LINE_COLOR")>
        Public Property CustomLineColor() As Color
            Get
                Return Me.m_clrLine
            End Get
            Set(ByVal value As Color)
                If (value <> Me.m_clrLine) Then
                    Me.m_clrLine = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_LINK"),
            cLocalizedDisplayName("HEADER_LINE_TYPE"),
            DefaultValue(eFDConnectionType.Arch)>
        Public Property LineConnectionType() As eFDConnectionType
            Get
                Return Me.m_connectiontype
            End Get
            Set(ByVal value As eFDConnectionType)
                If (value <> Me.m_connectiontype) Then
                    Me.m_connectiontype = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_SHOW_VALUES"),
            DefaultValue(False)>
        Public Property NodeDrawValue() As Boolean
            Get
                Return Me.m_bIsNodeDrawValue
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bIsNodeDrawValue) Then
                    Me.m_bIsNodeDrawValue = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("HEADER_SHOW_LABELS"),
            DefaultValue(True)>
        Public Property NodeDrawLabels() As Boolean
            Get
                Return Me.m_bIsDrawLabel
            End Get
            Set(ByVal value As Boolean)
                If (value <> Me.m_bIsDrawLabel) Then
                    Me.m_bIsDrawLabel = value
                    Me.Update()
                End If
            End Set
        End Property

        <Browsable(True),
            cLocalizedCategory("HEADER_COMPARTMENT"),
            cLocalizedDisplayName("GENERIC_SHOW_HIDDEN"),
            DefaultValue(eFDShowHiddenType.GrayedOut)>
        Public Property ShowHiddenMode() As eFDShowHiddenType _
            Implements IFlowDiagramRenderer.ShowHiddenNodes
            Get
                Return Me.m_nodeshowtype
            End Get
            Set(ByVal value As eFDShowHiddenType)
                If (value <> Me.m_nodeshowtype) Then
                    Me.m_nodeshowtype = value
                    Me.Update()
                End If
            End Set
        End Property

#End Region ' Configuration

#Region " Public access "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Save renderer settings.
        ''' </summary>
        ''' <param name="settings">The <see cref="cXMLSettings">settings</see> to save to.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Function Save(ByVal settings As cXMLSettings) As Boolean

            Try

                settings.SaveSetting("Appearance", "ShowTitle", Me.ShowTitle.ToString())
                settings.SaveSetting("Appearance", "ShowLegend", Me.ShowLegend.ToString())
                settings.SaveSetting("Appearance", "ShowHiddenNodes", Me.ShowHiddenMode.ToString())
                settings.SaveSetting("Appearance", "NumTL", CStr(Me.NumberOfTrophicLevels))
                settings.SaveSetting("Appearance", "ColorUsage", Me.AutoColorUsage.ToString())
                settings.SaveSetting("Appearance", "NodeType", Me.NodeType.ToString())
                settings.SaveSetting("Appearance", "NodeColor", CStr(cColorUtils.ColorToInt(Me.CustomNodeColor)))
                settings.SaveSetting("Appearance", "NodeAutosize", Me.AutoNodeSize.ToString())
                settings.SaveSetting("Appearance", "NodeSize", Me.CustomNodeSize.ToString())
                settings.SaveSetting("Appearance", "LineAutowidth", Me.AutoLineWidth.ToString())
                settings.SaveSetting("Appearance", "LineWidth", Me.CustomLineWidth.ToString())
                settings.SaveSetting("Appearance", "LineColor", CStr(cColorUtils.ColorToInt(Me.CustomLineColor)))

                settings.Flush()

            Catch ex As Exception
                ' ToDo: send an error message
                cLog.Write(ex, "cTreeFlowDiagramRenderer.Save")
                Return False
            End Try
            Return True

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load renderer settings.
        ''' </summary>
        ''' <param name="settings">The <see cref="cXMLSettings">settings</see> to load from.</param>
        ''' -------------------------------------------------------------------
        Public Overridable Function Load(ByVal settings As cXMLSettings) As Boolean

            Dim bSuccess As Boolean = True

            Me.m_bInUpdate = True

            Try

                Boolean.TryParse(settings.GetSetting("Appearance", "ShowTitle", CStr(True)), Me.ShowTitle)
                [Enum].TryParse(settings.GetSetting("Appearance", "ShowLegend", CStr(TriState.UseDefault)), Me.ShowLegend)
                [Enum].TryParse(settings.GetSetting("Appearance", "ShowHiddenNodes", CStr(eFDShowHiddenType.Invisible)), Me.ShowHiddenMode)
                Integer.TryParse(settings.GetSetting("Appearance", "NumTL", "5"), Me.NumberOfTrophicLevels)
                [Enum].TryParse(settings.GetSetting("Appearance", "ColorUsage", CStr(eFDColorUsageTypes.EwE)), Me.AutoColorUsage)
                [Enum].TryParse(settings.GetSetting("Appearance", "NodeType", CStr(eFDNodeTypes.Circle)), Me.NodeType)
                Me.CustomNodeColor = cColorUtils.IntToColor(CInt(settings.GetSetting("Appearance", "NodeColor", CStr(cColorUtils.ColorToInt(Color.Blue)))))
                Boolean.TryParse(settings.GetSetting("Appearance", "NodeAutosize", CStr(True)), Me.AutoNodeSize)
                Integer.TryParse(settings.GetSetting("Appearance", "NodeSize", CStr(25)), Me.CustomNodeSize)
                Boolean.TryParse(settings.GetSetting("Appearance", "LineAutowidth", CStr(True)), Me.AutoLineWidth)
                Single.TryParse(settings.GetSetting("Appearance", "LineWidth", CStr(2)), Me.CustomLineWidth)
                Me.CustomLineColor = cColorUtils.IntToColor(CInt(settings.GetSetting("Appearance", "LineColor", CStr(cColorUtils.ColorToInt(Color.Silver)))))

            Catch ex As Exception
                ' ToDo: send an error message
                cLog.Write(ex, "cTreeFlowDiagramRenderer.Load")
                bSuccess = False
            End Try

            Me.m_bInUpdate = False
            Me.Update()

            Return bSuccess

        End Function

        Public Sub CenterLabels()
            For i As Integer = 0 To Me.m_asLabelOffsetX.Length - 1
                Me.m_asLabelOffsetX(i) = 0
            Next
            Me.Update()
        End Sub

        Public Sub ResetLayout()
            Me.InitNodePositions()
            Me.Update()
        End Sub

        Public Property NodeLocation(ByVal i As Integer, ByVal rc As Rectangle) As PointF _
            Implements IFlowDiagramRenderer.NodeLocation
            Get
                Dim pt As PointF
                pt.X = CSng(Me.m_sAngle(i) / 360 * (rc.Width - 40)) + 20
                pt.Y = (Me.m_iNumTrophicLevels - Me.m_data.TrophicLevel(i)) * CInt(rc.Height / Me.m_iNumTrophicLevels)
                Return pt
            End Get
            Set(ByVal value As PointF)
                Dim angVal As Single = CSng((value.X - 20) / (rc.Width - 40) * 360)
                Me.m_sAngle(i) = Math.Max(0.0!, Math.Min(360.0!, angVal))
            End Set
        End Property

        Public Property LabelLocation(ByVal i As Integer, ByVal rc As Rectangle) As PointF _
            Implements IFlowDiagramRenderer.LabelLocation
            Get
                Dim ptfNode As PointF = Me.NodeLocation(i, rc)
                Return New PointF(ptfNode.X + Me.m_asLabelOffsetX(i), ptfNode.Y + Me.m_asLabelOffsetY(i))
            End Get
            Set(ByVal value As PointF)
                Dim ptfNode As PointF = Me.NodeLocation(i, rc)
                Me.m_asLabelOffsetX(i) = value.X - ptfNode.X
                Me.m_asLabelOffsetY(i) = value.Y - ptfNode.Y
            End Set
        End Property

        Public Function IsNodeAtPoint(ByVal rc As Rectangle, ByVal ptfTest As PointF,
                                      ByVal i As Integer, ByVal sValue As Single) As Boolean _
            Implements IFlowDiagramRenderer.IsNodeAtPoint

            Dim ptfNodeLocation As PointF = Me.NodeLocation(i, rc)
            Dim sNodeSize As Single = CSng(Math.Max(g_minsize, Me.CalcNodeSize(sValue, Me.m_data.ValueMax)))
            Dim rcf As New RectangleF(ptfNodeLocation.X - sNodeSize / 2,
                                      ptfNodeLocation.Y - sNodeSize / 2,
                                      sNodeSize,
                                      sNodeSize)

            Return rcf.Contains(ptfTest)

        End Function

        Public Function IsLabelAtPoint(ByVal rc As Rectangle,
                                       ByVal ptfTest As PointF,
                                       ByVal i As Integer,
                                       ByVal strLabel As String,
                                       ByVal g As Graphics,
                                       ByVal font As Font) As Boolean _
            Implements IFlowDiagramRenderer.IsLabelAtPoint

            Dim ptfLabelLocation As PointF = Me.LabelLocation(i, rc)
            Dim szfLabel As SizeF = Me.m_node.CalcLabelSize(g, font, strLabel, cTreeFlowDiagramRenderer.g_fmt)

            szfLabel.Width = Math.Max(szfLabel.Width, g_minsize)
            szfLabel.Height = Math.Max(szfLabel.Height, g_minsize)

            Dim rcf As New RectangleF(ptfLabelLocation.X - szfLabel.Width / 2, ptfLabelLocation.Y, szfLabel.Width, szfLabel.Height)

            Return rcf.Contains(ptfTest)

        End Function

#End Region ' Properties

#Region " EwE styling "

        Public Function RenderFont() As Font _
            Implements IFlowDiagramRenderer.RenderFont

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Return sg.Font(cStyleGuide.eApplicationFontType.Scale)

        End Function

        Public Function TextColor() As Color _
            Implements IFlowDiagramRenderer.TextColor

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Return sg.ApplicationColor(cStyleGuide.eApplicationColorType.DEFAULT_TEXT)

        End Function

        Public Function InLinkColor() As Color _
            Implements IFlowDiagramRenderer.InLinkColor

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Return sg.ApplicationColor(cStyleGuide.eApplicationColorType.PREY)

        End Function

        Public Function OutLinkColor() As Color _
            Implements IFlowDiagramRenderer.OutLinkColor

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Return sg.ApplicationColor(cStyleGuide.eApplicationColorType.PREDATOR)

        End Function

        Public Function HighlightColor() As Color _
            Implements IFlowDiagramRenderer.HighlightColor

            Dim sg As cStyleGuide = Me.UIContext.StyleGuide
            Return sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)

        End Function

        Public Function FormatLabelText(ByVal iGroup As Integer) As String _
            Implements IFlowDiagramRenderer.FormatLabelText

            Dim sb As New StringBuilder()
            Dim sValue As Single = Me.m_data.Value(iGroup)
            Dim strName As String = Me.m_data.ItemName(iGroup)

            sb.AppendLine(strName)
            If Me.m_bIsNodeDrawValue And (sValue <> 0.0!) Then
                sb.AppendLine(Me.m_data.ValueLabel(sValue))
            End If
            Return sb.ToString

        End Function

#End Region ' EwE styling

#Region " Internals "

        Protected Sub Update()
            If Me.m_bInUpdate Then Return
            Try
                RaiseEvent OnChanged(Me)
            Catch ex As Exception
                Debug.Assert(False, ex.Message)
            End Try
        End Sub

        Private ReadOnly Property CalcNodeSize(ByVal sValue As Single, ByVal sValueMax As Single) As Integer
            Get
                Dim iSize As Integer = Me.m_iNodeSize

                If (Me.m_bAutoNodeSize) And (sValue > 0) And (sValueMax > 0) Then
                    Select Case Me.NodeScaleType
                        Case eFDNodeScaleType.Logarithmic
                            ' Ln(values 1-11) make max ~2.5 => times 4 to scale to 10
                            ' Note that Math.Log = ln
                            iSize = CInt(Math.Log(1.2 + (10 * sValue / sValueMax)) * (1.2 * iSize))
                        Case eFDNodeScaleType.Linear, eFDNodeScaleType.SquareRoot

                            If (Me.NodeScaleType = eFDNodeScaleType.SquareRoot) Then
                                sValue = CSng(Math.Sqrt(sValue))
                                sValueMax = CSng(Math.Sqrt(sValueMax))
                            End If

                            Select Case Me.m_nodetype
                                Case eFDNodeTypes.Circle
                                    iSize = CInt(Me.m_iNodeSize * Math.Sqrt((2 * sValue / sValueMax) / Math.PI) * 2)
                                Case eFDNodeTypes.Rectangle
                                    iSize = CInt(Me.m_iNodeSize * Math.Sqrt(2 * sValue / sValueMax))
                            End Select
                        Case Else
                            Debug.Assert(False)
                    End Select
                End If
                Return Math.Max(3, iSize)
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns the size of a line in the diagram.
        ''' </summary>
        ''' <param name="sValue">Value represented by the line.</param>
        ''' <param name="sValueMax">Max value represented by all lines.</param>
        ''' -------------------------------------------------------------------
        Private ReadOnly Property CalcLineWidth(ByVal sValue As Single, ByVal sValueMax As Single) As Integer
            Get
                Dim sLineSize As Single = Me.m_sLineWidth

                If Me.m_bAutoLineWidth Then
                    If sValueMax > 0 And sValue > 0 Then
                        sLineSize = CSng(Math.Log(1.2 + (10 * sValue / sValueMax)) * 4) ' Log(values 1-11) make max ~2.5 => times 4 to scale to 10
                    End If
                End If

                Return CInt(Math.Min(Math.Max(1, sLineSize), 10))
            End Get
        End Property

#End Region ' Internals

    End Class

End Namespace