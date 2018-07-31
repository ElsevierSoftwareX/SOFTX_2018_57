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

Imports System.ComponentModel
Imports EwECore
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Style
Imports ZedGraph

#End Region

Namespace Controls

    ''' <summary>
    ''' User control for showing mediation effect assignments.
    ''' </summary>
    Public Class ucMediationAssignments
        Implements IUIElement

#Region " Private vars "

        Private m_uic As cUIContext = Nothing
        Private m_medfn As cMediationBaseFunction = Nothing
        Private m_zgh As cZedGraphMediationHelper 'cZedGraphHelper = Nothing
        Private m_strXAxisLabel As String = ""
        Private m_strYAxisLabel As String = ""
        Private m_strTitle As String = ""
        Private m_data As cBioPercentData = Nothing
        Private m_viewmode As eViewModeTypes = eViewModeTypes.Pie

#End Region ' Private vars

#Region " Framework overrides "

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Protected Overrides Sub DestroyHandle()
            Me.UIContext = Nothing
            MyBase.DestroyHandle()
        End Sub

#End Region ' Framework overrides

#Region " Events "

        Protected Overridable Sub OnStyleGuideChanged(ByVal change As cStyleGuide.eChangeType)
            ' Refresh regardless of the type of change
            Me.LoadGraphData(Me.m_data)
        End Sub

#End Region ' Events

#Region " Public interfaces "

        Public Enum eViewModeTypes As Integer
            Pie = 0
            Bar
            Line
        End Enum

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Data structure for showing data in this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Class cBioPercentData
            Public Groups() As cMediatingGroup
            Public Fleets() As cMediatingFleet
        End Class

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Refresh the content of this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub RefreshContent()

            If (Me.m_medfn IsNot Nothing) Then
                Me.m_data = Me.ExtractData(Me.m_medfn)
            End If
            Me.LoadGraphData(Me.m_data)
            Me.UpdateControls()

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cMediationBaseFunction"/> to display in this control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Property Shape() As cMediationBaseFunction
            Get
                Return Me.m_medfn
            End Get
            Set(ByVal value As cMediationBaseFunction)
                ' Store function ref
                Me.m_medfn = DirectCast(value, cMediationBaseFunction)
                ' Reload content
                Me.RefreshContent()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the <see cref="cBioPercentData"/> to display in this control.
        ''' </summary>
        ''' <remarks>
        ''' This data is automatically extracted when a <see cref="Shape"/> is provided.
        ''' </remarks>
        ''' -------------------------------------------------------------------
        <Browsable(False)> _
        Public Property Data() As cBioPercentData
            Get
                Return Me.m_data
            End Get
            Set(ByVal value As cBioPercentData)
                Me.m_data = value
                Me.RefreshContent()
            End Set
        End Property

        <Browsable(False)> _
        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)

                If m_uic IsNot Nothing Then
                    RemoveHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                    Me.m_zgh.Detach()
                    Me.m_zgh = Nothing
                End If

                Me.m_uic = value

                If Me.m_uic IsNot Nothing Then
                    Me.m_zgh = New cZedGraphMediationHelper 'cZedGraphHelper()
                    Me.m_zgh.Attach(Me.UIContext, Me.m_zedgraph, 1)
                    Me.LoadGraphData(Me.m_data)

                    If Me.m_viewmode = eViewModeTypes.Line Then
                        'ONLY show toolTips when in Environmental Response mode??
                        Me.m_zgh.ShowPointValue = True
                    End If

                    AddHandler Me.m_uic.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the X-axis label for the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Category("Mediation"), _
         Description("Label to display on the Y axis")> _
        Public Property XAxisLabel() As String
            Get
                Return Me.m_strXAxisLabel
            End Get
            Set(ByVal value As String)
                Me.m_strXAxisLabel = value
                If Me.m_zgh IsNot Nothing Then
                    Me.m_zgh.GetPane(1).XAxis.Title.Text = Me.m_strXAxisLabel
                End If
                Me.UpdateControls()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the Y-axis label for the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Category("Mediation"), _
         Description("Label to display on the X axis")> _
        Public Property YAxisLabel() As String
            Get
                Return Me.m_strYAxisLabel
            End Get
            Set(ByVal value As String)
                Me.m_strYAxisLabel = value
                If Me.m_zgh IsNot Nothing Then
                    Me.m_zgh.GetPane(1).YAxis.Title.Text = Me.m_strYAxisLabel
                End If
                Me.UpdateControls()
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the X-axis label for the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Category("Mediation"), _
         Description("Data view mode")> _
        Public Property ViewMode() As eViewModeTypes
            Get
                Return Me.m_viewmode
            End Get
            Set(ByVal value As eViewModeTypes)
                Me.m_viewmode = value
                ' Whoah
                Me.UIContext = Me.UIContext
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the title for the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        <Browsable(True), _
         Category("Mediation"), _
         Description("Graph title")> _
        Public Property Title() As String
            Get
                Return Me.m_strTitle
            End Get
            Set(ByVal value As String)
                Me.m_strTitle = value
                If Me.m_zgh IsNot Nothing Then
                    Me.m_zgh.GetPane(1).Title.Text = Me.m_strTitle
                    Me.m_zgh.GetPane(1).Title.IsVisible = Not String.IsNullOrEmpty(Me.m_strTitle)
                    Me.m_zedgraph.Invalidate()
                    Me.UpdateControls()
                End If
            End Set
        End Property

#End Region ' Public interfaces

#Region " Internals "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Extract <see cref="cBioPercentData"/> from the current attached <see cref="Shape"/>.
        ''' </summary>
        ''' <param name="fn"></param>

        Private Function ExtractData(ByVal fn As cMediationBaseFunction) As cBioPercentData

            Dim d As New cBioPercentData()

            Dim lGroups As New List(Of cMediatingGroup)
            For iIndex As Integer = 0 To fn.NumGroups - 1
                lGroups.Add(fn.Group(iIndex))
            Next
            d.Groups = lGroups.ToArray()

            Dim lFleets As New List(Of cMediatingFleet)
            For iIndex As Integer = 0 To fn.NumFleet - 1
                lFleets.Add(fn.Fleet(iIndex))
            Next
            d.Fleets = lFleets.ToArray()

            Return d

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Load the graph with external data, for UI preview purposes.
        ''' </summary>
        ''' <param name="data"></param>
        ''' -------------------------------------------------------------------
        Public Sub LoadGraphData(ByVal data As cBioPercentData)

            Me.m_data = data

            If (data IsNot Nothing) Then
                Select Case Me.m_viewmode
                    Case eViewModeTypes.Pie
                        Me.LoadAsPie()
                    Case eViewModeTypes.Bar
                        Me.LoadAsBar()
                    Case eViewModeTypes.Line
                        Me.LoadAsLine()
                End Select
                Me.m_zedgraph.Visible = True
            Else
                m_zedgraph.Visible = False
            End If

            ' Calculate the Axis Scale Ranges
            m_zedgraph.AxisChange()
            m_zedgraph.Refresh()

        End Sub


        Public Sub LoadAsLine()

            Try
                ' Sanity checks
                If (Me.m_uic Is Nothing) Then Return
                If (Me.IsDisposed) Then Return
                If (Me.m_medfn Is Nothing) Then Return

                Dim sg As cStyleGuide = Me.m_uic.StyleGuide
                Dim list As PointPairList = Nothing
                Dim pane As GraphPane = Nothing
                Dim source As cCoreInputOutputBase = Nothing
                Dim strLabel As String = ""
                Dim fmt As New cCoreInterfaceFormatter()
                Dim clr As Color = Color.Transparent

                Me.m_zgh.ConfigurePane(Me.m_strTitle, Me.m_strXAxisLabel, Me.m_strYAxisLabel, True)
                pane = Me.m_zgh.GetPane(1)

                pane.XAxis.Scale.IsVisible = True
                pane.CurveList.Clear()

                'make sure this is the correct type of shape
                Debug.Assert(TypeOf Me.m_medfn Is cEnviroResponseFunction, Me.ToString & ".LoadAsLine() Invalid shape type.")
                If Not (TypeOf Me.m_medfn Is cEnviroResponseFunction) Then
                    Exit Sub
                End If
                Dim resShape As cEnviroResponseFunction = DirectCast(Me.m_medfn, cEnviroResponseFunction)

                'X Axis is defined by the shape itself
                Dim Xmax As Single = resShape.ResponseRightLimit
                Dim Xmin As Single = resShape.ResponseLeftLimit
                'Scale the Y axis to one
                Dim YScale As Single = 1 ' / resShape.YMax
                If Xmax = 0 Then Xmax = 1
                Dim Xrange As Single = Xmax - Xmin

                Dim dx As Single = Xrange / resShape.nPoints
                If dx = 0 Then dx = 1
                Dim lstPts As New PointPairList

                'First point from shape at the zero X axis
                lstPts.Add(Xmin, resShape.ShapeData(1) * YScale)
                For ipt As Integer = 1 To resShape.nPoints
                    lstPts.Add(Xmin + dx * (ipt - 1), resShape.ShapeData(ipt) * YScale)
                Next

                ''add the last point out at the end of the graph
                'lstPts.Add(Xmax, resShape.ShapeData(resShape.XMax))

                'need a way to find the color of the shape
                Dim il As LineItem = Me.m_zgh.CreateLineItem(cStringUtils.Localize(My.Resources.HEADER_RESPONSE_TARGET, fmt.GetDescriptor(resShape)), _
                                                             lstPts, cZedGraphMediationHelper.eEnvResponseLineType.Response)
                pane.CurveList.Add(il)

                Me.m_zgh.XScaleMax = Xmax
                Me.m_zgh.YScaleMax = resShape.YMax * 1.1
                Me.m_zgh.YScaleMin = 0

            Catch ex As Exception
                Debug.Assert(False)
                System.Console.WriteLine(Me.ToString & ".LoadAsLine() Exception " & ex.Message)
            End Try

        End Sub

        Public Sub LoadAsBar()

            ' Sanity checks
            If (Me.m_uic Is Nothing) Then Return
            If (Me.IsDisposed) Then Return

            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim medGrp As cMediatingGroup = Nothing
            Dim medFlt As cMediatingFleet = Nothing
            Dim list As PointPairList = Nothing
            Dim pane As GraphPane = Nothing
            Dim source As cCoreInputOutputBase = Nothing
            Dim strLabel As String = ""
            Dim fmt As New cCoreInterfaceFormatter()
            Dim clr As Color = Color.Transparent
            Dim myCurve As BarItem = Nothing

            Me.m_zgh.ConfigurePane(Me.m_strTitle, Me.m_strXAxisLabel, Me.m_strYAxisLabel, True)
            pane = Me.m_zgh.GetPane(1)

            pane.XAxis.Scale.IsVisible = False
            pane.CurveList.Clear()

            For i As Integer = 0 To Data.Groups.Length - 1
                list = New PointPairList()
                medGrp = Data.Groups(i)
                list.Add(i + 1, medGrp.Weight)

                ' Get the group
                source = Me.m_uic.Core.EcoPathGroupInputs(medGrp.iGroupIndex)
                clr = sg.GroupColor(Me.m_uic.Core, medGrp.iGroupIndex)

                If (TypeOf medGrp Is cLandingsMediatingGroup) Then
                    Dim medLandings As cLandingsMediatingGroup = DirectCast(medGrp, cLandingsMediatingGroup)
                    ' Is a landings interaction?
                    If (medLandings.iFleetIndex > 0) Then
                        Dim sourceSec As cCoreInputOutputBase = Me.m_uic.Core.EcopathFleetInputs(medLandings.iFleetIndex)
                        strLabel = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, _
                                                 fmt.GetDescriptor(source), _
                                                 fmt.GetDescriptor(sourceSec))
                    Else
                        strLabel = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DOUBLE, _
                                                 fmt.GetDescriptor(source), _
                                                 My.Resources.GENERIC_VALUE_ALL)
                    End If
                Else
                    strLabel = fmt.GetDescriptor(source)
                End If
                myCurve = pane.AddBar(strLabel, list, clr)
                myCurve.Bar.Fill = New Fill(clr)

            Next

            For i As Integer = 0 To Data.Fleets.Length - 1
                list = New PointPairList()
                medFlt = Data.Fleets(i)
                list.Add(i + 1 + Data.Groups.Length, medFlt.Weight)

                ' Get the fleet
                source = Me.m_uic.Core.EcopathFleetInputs(medFlt.iFleetIndex)
                clr = sg.FleetColor(Me.m_uic.Core, medFlt.iFleetIndex)
                strLabel = fmt.GetDescriptor(source)

                myCurve = pane.AddBar(strLabel, list, clr)
                myCurve.Bar.Fill = New Fill(clr)
            Next

        End Sub

        Private Sub LoadAsPie()

            ' Sanity checks
            If (Me.m_uic Is Nothing) Then Return
            If (Me.IsDisposed) Then Return

            Dim sg As cStyleGuide = Me.m_uic.StyleGuide
            Dim medGrp As cMediatingGroup = Nothing
            Dim medFlt As cMediatingFleet = Nothing
            Dim list As PointPairList = Nothing
            Dim pane As GraphPane = Nothing
            Dim valSource As cCoreInputOutputBase = Nothing
            Dim strLabel As String = ""
            Dim fmt As New cCoreInterfaceFormatter()
            Dim clr As Color = Color.Transparent
            Dim myCurve As BarItem = Nothing
            Dim varname As EwEUtils.Core.eVarNameFlags
            Dim iGroup As Integer

            Me.m_zgh.ConfigurePane(Me.m_strTitle, "", "", True)
            pane = Me.m_zgh.GetPane(1)

            pane.XAxis.Scale.IsVisible = False
            pane.CurveList.Clear()

            For i As Integer = 0 To Data.Groups.Length - 1

                ' Get the group
                medGrp = Data.Groups(i)
                valSource = Me.m_uic.Core.EcoPathGroupOutputs(medGrp.iGroupIndex)

                ' Is a landings interaction?
                If (TypeOf medGrp Is cLandingsMediatingGroup) Then
                    ' #Yes
                    Dim medLandings As cLandingsMediatingGroup = DirectCast(medGrp, cLandingsMediatingGroup)

                    ' Group index and VarName for the landings of this group by this fleet
                    iGroup = medLandings.iGroupIndex

                    If (medLandings.iFleetIndex > 0) Then
                        Dim FleetSource As cCoreInputOutputBase = Me.m_uic.Core.EcopathFleetInputs(medLandings.iFleetIndex)
                        strLabel = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DETAILED, _
                                                 fmt.GetDescriptor(valSource), _
                                                 fmt.GetDescriptor(FleetSource))

                        'Ok this is a little strange
                        'set the source of the values to the FleetInput 
                        valSource = FleetSource
                        varname = EwEUtils.Core.eVarNameFlags.Landings

                    Else
                        strLabel = cStringUtils.Localize(My.Resources.GENERIC_LABEL_DOUBLE, _
                                                 fmt.GetDescriptor(valSource), _
                                                 My.Resources.GENERIC_VALUE_ALL)
                    End If
                Else
                    'Biomass 
                    strLabel = fmt.GetDescriptor(valSource)
                    iGroup = cCore.NULL_VALUE
                    varname = EwEUtils.Core.eVarNameFlags.Biomass

                End If

                clr = sg.GroupColor(Me.m_uic.Core, medGrp.iGroupIndex)

                If varname <> EwEUtils.Core.eVarNameFlags.NotSet Then

                    Dim sliceVal As Double = CDbl(valSource.GetVariable(varname, iGroup)) * medGrp.Weight
                    Dim slice As PieItem = pane.AddPieSlice(sliceVal, clr, 0.05, strLabel)
                    slice.ValueDecimalDigits = sg.NumDigits
                    slice.LabelType = PieLabelType.Value

                End If

            Next

            ' Configure pane
            pane.Legend.IsVisible = True
            pane.Legend.Position = LegendPos.Right
            pane.Legend.IsHStack = False

        End Sub

        Private Sub UpdateControls()

            ' Only show graph when it has data
            If (Me.m_zedgraph IsNot Nothing) And (Me.m_zgh IsNot Nothing) Then
                Dim p As GraphPane = Me.m_zgh.GetPane(1)
                Dim bHasData As Boolean = (p.CurveList.Count > 0)
                Me.m_zedgraph.Visible = bHasData
            End If

        End Sub

#End Region ' Internals

    End Class

End Namespace



