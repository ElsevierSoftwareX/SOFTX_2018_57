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
Option Explicit On

Imports EwECore
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports 

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add-on class that implements EwEcolour and display feedback on Windows
    ''' controls.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cEwEFormatProvider
        Implements IUIElement

#Region " Helper classes "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Factory to generate an <see cref="IControlWrapper">IControlWrapper</see>
        ''' for a given Windows control.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cControlWrapperFactory

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Factory method; instantiates a <see cref="IControlWrapper">IControlWrapper</see>
            ''' for a given Windows control.
            ''' </summary>
            ''' <param name="ctrl">The <see cref="Control">Windows Control</see> to wrap.</param>
            ''' <param name="provider">The <see cref="cEwEFormatProvider">cEwEFormatProvider</see>
            ''' that requested this wrap.</param>
            ''' <returns>A <see cref="IControlWrapper">IControlWrapper</see> instance if successful,
            ''' or nothing if an error occurred.</returns>
            ''' -----------------------------------------------------------------------
            Shared Function GetControlWrapper(ByVal uic As cUIContext, _
                                              ByVal ctrl As Control, _
                                              ByVal provider As cEwEFormatProvider, _
                                              Optional ByVal aItems As Object() = Nothing, _
                                              Optional ByVal metadata As cVariableMetaData = Nothing, _
                                              Optional formatter As ITypeFormatter = Nothing) As IControlWrapper

                Dim wrapper As IControlWrapper = Nothing

                ' Wrapper supported Windows controls
                If TypeOf (ctrl) Is System.Windows.Forms.TextBox Or TypeOf (ctrl) Is System.Windows.Forms.RichTextBox Then
                    wrapper = New cTextBoxWrapper()
                ElseIf TypeOf (ctrl) Is Label Then
                    wrapper = New cLabelWrapper()
                ElseIf TypeOf (ctrl) Is CheckBox Then
                    wrapper = New cCheckboxWrapper()
                ElseIf TypeOf (ctrl) Is ComboBox Then
                    wrapper = New cComboBoxWrapper()
                ElseIf TypeOf (ctrl) Is NumericUpDown Then
                    wrapper = New cNumericUpDownWrapper()
                End If

                ' Development time sanity check
                Debug.Assert(wrapper IsNot Nothing, cStringUtils.Localize("ControlWrapperFactory: control {0} not supported", ctrl.GetType().ToString()))

                ' Pass on UI context
                wrapper.UIContext = uic
                ' Try to wrap
                If Not wrapper.Wrap(ctrl, provider, aItems, metadata, formatter) Then wrapper = Nothing
                ' Return result
                Return wrapper

            End Function

        End Class

#Region " Private helper classes "

#Region " Interface IControlWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Interface for wrapping standard Windows control by an EwEFormatProvider.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Public Interface IControlWrapper
            Inherits IUIElement

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Wrap a Windows control by attaching it to an EwEFormatProvider.
            ''' </summary>
            ''' <param name="ctrl">The <see cref="Control">Control</see> to interact with.</param>
            ''' <param name="provider">The <see cref="cEwEFormatProvider">cEwEFormatProvider</see>
            ''' that will provide <see cref="cEwEFormatProvider.Value">value</see>, 
            ''' <see cref="cEwEFormatProvider.ValueType">value type</see> and 
            ''' <see cref="cEwEFormatProvider.Style">display style</see> for the control.</param>
            ''' <returns>True if wrapped succesfully.</returns>
            ''' -----------------------------------------------------------------------
            Function Wrap(ByVal ctrl As Control, _
                         ByVal provider As cEwEFormatProvider, _
                         Optional ByVal aItems As Object() = Nothing, _
                         Optional ByVal metadata As cVariableMetaData = Nothing, _
                         Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Release a wrapped control.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Sub Release()

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Trigger to update the value and display style of the <see cref="Control">Control</see>.
            ''' </summary>
            ''' <param name="cf">Aspect to update.</param>
            ''' -----------------------------------------------------------------------
            Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags)

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' List of items to reflect in the control. This paramter will only work
            ''' for list-containing controls such as combo boxes and list boxes.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Property Items() As Object()

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get the wrapped control.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            ReadOnly Property Control As Control

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Get the metadata that the control was wrapped with.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            ReadOnly Property Metadata As cVariableMetaData

        End Interface

#End Region ' Interface IControlWrapper

#Region " Class TextBoxWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cTextBoxWrapper
            Implements IControlWrapper

#Region " Private variables "

            ''' <summary>The wrapped text box</summary>
            Private m_tb As TextBoxBase = Nothing
            ''' <summary>The EwEFormatProvider that implements value and colour
            ''' behaviour onto the text box.</summary>
            Private m_provider As cEwEFormatProvider = Nothing

            Private m_md As cVariableMetaData = Nothing

#End Region ' Private variables 

#Region " Implementation "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the UI context for this wrapper.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property UIContext() As cUIContext _
                Implements IUIElement.UIContext

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Wrap control as a TextBoxBase class
            ''' </summary>
            ''' <param name="ctrl"></param>
            ''' <param name="provider"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Public Function Wrap(ByVal ctrl As Control, _
                                ByVal provider As cEwEFormatProvider, _
                                Optional ByVal aItems() As Object = Nothing, _
                                Optional ByVal metadata As EwECore.cVariableMetaData = Nothing, _
                                Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean _
                Implements IControlWrapper.Wrap

                Dim objValueType As Object = provider.ValueType
                Dim bSucces As Boolean = True

                Try
                    ' Store ref to Text box
                    Me.m_tb = DirectCast(ctrl, TextBoxBase)
                    AddHandler Me.m_tb.Enter, AddressOf OnControlEntered
                    AddHandler Me.m_tb.Leave, AddressOf OnControlLeft
                    AddHandler Me.m_tb.KeyDown, AddressOf OnControlKeyDown

                    Me.m_provider = provider
                    Me.m_md = metadata

                Catch e As Exception
                    ' Throw dev. time error
                    Debug.Assert(False, "Failed to wrap text box")
                    ' Report failure
                    bSucces = False
                End Try

                Return bSucces

            End Function

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Release a wrapped control.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub Release() _
                    Implements IControlWrapper.Release

                If (Me.m_tb IsNot Nothing) Then
                    RemoveHandler Me.m_tb.Enter, AddressOf OnControlEntered
                    RemoveHandler Me.m_tb.LostFocus, AddressOf OnControlLeft
                    RemoveHandler Me.m_tb.KeyDown, AddressOf OnControlKeyDown
                    Me.m_tb = Nothing
                End If

            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Update value and display style of the text box.
            ''' </summary>
            ''' <param name="cf">Aspect of the text box to change.</param>
            ''' -----------------------------------------------------------------------
            Public Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags) _
                Implements IControlWrapper.UpdateContent

                Dim sg As cStyleGuide = Me.UIContext.StyleGuide
                Dim objValue As Object = Me.m_provider.Value
                Dim objValueType As Object = Me.m_provider.ValueType
                Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                Dim bEditable As Boolean = ((style And cStyleGuide.eStyleFlags.NotEditable) = 0)
                Dim bVisible As Boolean = ((style And cStyleGuide.eStyleFlags.Null) = 0)
                Dim strText As String = ""

                ' Apply metadata
                If (Me.m_md IsNot Nothing) Then
                    Me.m_tb.MaxLength = Metadata.Length
                End If

                If (cf And (Properties.cProperty.eChangeFlags.Value Or Properties.cProperty.eChangeFlags.CoreStatus)) > 0 Then

                    ' Sanity checks
                    If (objValue IsNot Nothing) Then

                        ' Get default value
                        strText = objValue.ToString()

                        ' Interpret as single?
                        If objValueType Is GetType(Single) Then
                            ' #Yes: apply format
                            If Me.m_tb.Focused Then
                                strText = CDbl(objValue).ToString
                            Else
                                strText = sg.FormatNumber(CSng(objValue), style)
                            End If
                        End If

                        ' Interpret as double?
                        If objValueType Is GetType(Double) Then
                            ' #Yes: apply format
                            If Me.m_tb.Focused Then
                                strText = CDbl(objValue).ToString
                            Else
                                strText = sg.FormatNumber(CDbl(objValue), style)
                            End If
                        End If
                    End If

                    ' - Set colours
                    sg.GetStyleColors(style, Me.m_tb.ForeColor, Me.m_tb.BackColor)

                    ' - Set read-only state
                    Me.m_tb.ReadOnly = (bEditable = False)
                    Me.m_tb.TabStop = (bEditable = True)

                    ' Highlight border
                    If (style And cStyleGuide.eStyleFlags.Highlight) > 0 Then
                        Me.m_tb.BackColor = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                    End If

                    If Not bVisible Then
                        strText = ""
                    End If

                    Me.m_tb.Text = strText

                End If


            End Sub

            Public Property Items() As Object() Implements IControlWrapper.Items
                Get
                    Return Nothing
                End Get
                Set(ByVal value As Object())
                End Set
            End Property

            Public ReadOnly Property Control As System.Windows.Forms.Control Implements IControlWrapper.Control
                Get
                    Return Me.m_tb
                End Get
            End Property

            Public ReadOnly Property Metadata As EwECore.cVariableMetaData Implements IControlWrapper.Metadata
                Get
                    Return Me.m_md
                End Get
            End Property

            Private Sub ProcessChanges()
                If (Me.m_tb Is Nothing) Then Return

                Try
                    ' Did anything change?
                    Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                    Dim bEditable As Boolean = ((style And cStyleGuide.eStyleFlags.NotEditable) = 0)
                    'Dim bVisible As Boolean = ((style And cStyleGuide.eStyleFlags.Null) = 0)

                    If Me.m_tb.Modified And bEditable Then
                        ' Update internal value
                        Me.m_provider.Value = Me.m_tb.Text
                    End If
                    ' Clear modified flag
                    Me.m_tb.Modified = False
                    Me.UpdateContent(cProperty.eChangeFlags.Value)
                Catch ex As Exception

                End Try
            End Sub

#End Region ' Implementation

#Region " TextBox events "

            '''' -----------------------------------------------------------------------
            '''' <summary>
            '''' Event handler, invoked when the Text Box text was entered.
            '''' </summary>
            '''' -----------------------------------------------------------------------
            Private Sub OnControlEntered(ByVal sender As Object, ByVal e As System.EventArgs)
                Try
                    ' Refresh value
                    Me.UpdateContent(ScientificInterfaceShared.Properties.cProperty.eChangeFlags.Value)
                Catch ex As Exception
                    ' WHoah!
                End Try
            End Sub

            '''' -----------------------------------------------------------------------
            '''' <summary>
            '''' Event handler, invoked when the Text Box text has lost focus. This will 
            '''' pass the modified text back to the parent 
            '''' <see cref="EwEFormatProvider">EwEFormatProvider</see>.
            '''' </summary>
            '''' -----------------------------------------------------------------------
            Private Sub OnControlLeft(ByVal sender As Object, ByVal e As System.EventArgs)

                ProcessChanges()

            End Sub

            Private Sub OnControlKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
                If (e.KeyCode = Keys.Enter And Not Me.m_tb.Multiline) Then
                    Me.ProcessChanges()
                End If
            End Sub

#End Region ' TextBox events

        End Class

#End Region ' Class TextBoxWrapper

#Region " Class NumericUpDownWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Helper class; wraps a NumericUpDown control for interaction with a Property.
        ''' </summary>
        ''' <remarks>
        ''' Note that the up/down control is not truely suitable for handling EwE
        ''' variables; it cannot be emptied (to reflect NULL status values) and its
        ''' value ranges cannot be limited to reflect values such as 'greater than' and
        ''' 'less than' making the control unsuitable for displaying a range of EwE
        ''' values.
        ''' </remarks>
        ''' -----------------------------------------------------------------------
        Private Class cNumericUpDownWrapper
            Implements IControlWrapper

#Region " Private variables "

            ''' <summary></summary>
            Private m_ud As NumericUpDown = Nothing
            ''' <summary></summary>
            Private m_provider As cEwEFormatProvider = Nothing
            ''' <summary>For trapping number of decimal digits display.</summary>
            Private m_sg As cStyleGuide = Nothing

            Private m_md As cVariableMetaData = Nothing

#End Region ' Private variables 

#Region " Implementation "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the UI context for this wrapper.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property UIContext() As cUIContext _
                Implements IUIElement.UIContext

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="ctrl"></param>
            ''' <param name="provider"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Public Function Wrap(ByVal ctrl As Control, _
                                 ByVal provider As cEwEFormatProvider, _
                                 Optional ByVal aItems() As Object = Nothing, _
                                 Optional ByVal metadata As EwECore.cVariableMetaData = Nothing, _
                                 Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean _
                    Implements IControlWrapper.Wrap

                Dim objValueType As Object = provider.ValueType
                Dim bSucces As Boolean = True

                ' Test for incompatible data types
                ' - Strings
                If objValueType Is GetType(String) Then
                    Debug.Assert(False, "NumericUpDown controls cannot handle string values")
                    Return False
                End If
                ' - Booleans
                If objValueType Is GetType(Boolean) Then
                    Debug.Assert(False, "NumericUpDown controls are unsuitable for handling boolean values - use checkbox instead")
                    Return False
                End If

                Try
                    ' Store ref to control
                    Me.m_ud = DirectCast(ctrl, NumericUpDown)
                    AddHandler Me.m_ud.KeyDown, AddressOf OnKeyDown
                    AddHandler Me.m_ud.LostFocus, AddressOf OnValidate
                    AddHandler Me.m_ud.ValueChanged, AddressOf OnValidate

                    Me.m_sg = Me.UIContext.StyleGuide
                    AddHandler Me.m_sg.StyleGuideChanged, AddressOf OnStyleGuideChanged

                    Me.m_provider = provider
                    Me.m_md = metadata

                    ' Config control
                    Me.OnStyleGuideChanged(cStyleGuide.eChangeType.All)
                Catch e As Exception
                    ' Throw dev. time error
                    Debug.Assert(False, "Failed to wrap numeric up down control")
                    ' Report failure
                    bSucces = False
                End Try

                Return bSucces

            End Function

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Release a wrapped control.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub Release() _
                    Implements IControlWrapper.Release

                If (Me.m_ud IsNot Nothing) Then
                    RemoveHandler Me.m_ud.KeyDown, AddressOf OnKeyDown
                    RemoveHandler Me.m_ud.LostFocus, AddressOf OnValidate
                    RemoveHandler Me.m_ud.ValueChanged, AddressOf OnValidate
                    Me.m_ud = Nothing
                End If

                If (Me.m_sg IsNot Nothing) Then
                    RemoveHandler Me.m_sg.StyleGuideChanged, AddressOf OnStyleGuideChanged
                    Me.m_sg = Nothing
                End If

            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Update value and display style of the numeric up down control.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags) _
                Implements IControlWrapper.UpdateContent

                Dim objValue As Object = Me.m_provider.Value
                Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                Dim bEditable As Boolean = ((style And cStyleGuide.eStyleFlags.NotEditable) = 0)

                ' Apply metadata
                If (Metadata IsNot Nothing) Then
                    Me.m_ud.Minimum = CDec(Math.Max(-10000000000, CSng(Metadata.Min)))
                    Me.m_ud.Maximum = CDec(Math.Min(10000000000, CSng(Metadata.Max)))
                End If

                If (cf And Properties.cProperty.eChangeFlags.Value) > 0 Then

                    ' Sanity checks
                    If objValue Is Nothing Then Return

                    ' Update control
                    ' - Set value truncated to min and max ranges. Note that value_none is not supported here!
                    Me.m_ud.Value = Math.Max(Me.m_ud.Minimum, Math.Min(Me.m_ud.Maximum, Convert.ToDecimal(objValue)))

                End If

                If (cf And Properties.cProperty.eChangeFlags.CoreStatus) > 0 Then
                    ' - Set colours
                    Me.m_sg.GetStyleColors(style, Me.m_ud.ForeColor, Me.m_ud.BackColor)
                    ' - Set read-only state
                    ' JS 16Feb18: Fixed #1559; 'readonly' state still allows a NUD to be changed through the up/down buttons. Aargh
                    Me.m_ud.Enabled = bEditable
                    Me.m_ud.TabStop = bEditable

                    ' Highlight border
                    If (style And cStyleGuide.eStyleFlags.Highlight) > 0 Then
                        Me.m_ud.BackColor = Me.m_sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                    End If
                End If

            End Sub

            Public Property Items() As Object() Implements IControlWrapper.Items
                Get
                    Return Nothing
                End Get
                Set(ByVal value As Object())
                End Set
            End Property

            Public ReadOnly Property Control As System.Windows.Forms.Control Implements IControlWrapper.Control
                Get
                    Return Me.m_ud
                End Get
            End Property

            Public ReadOnly Property Metadata As EwECore.cVariableMetaData Implements IControlWrapper.Metadata
                Get
                    Return Me.m_md
                End Get
            End Property

#End Region ' Implementation

#Region " NumericUpDown events "

            Private Sub OnKeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs)
                Try
                    If (e.KeyCode = Keys.Enter) Then
                        Me.Validate()
                    End If
                Catch ex As Exception
                    cLog.Write(ex, eVerboseLevel.Detailed, "NumericUpDownWrapper:OnKeyDown")
                End Try
            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Event handler, invoked when the numeric up down control value has changed. 
            ''' This will pass the control value back into the parent <see cref="cEwEFormatProvider"/>.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub OnValidate(ByVal sender As Object, ByVal e As System.EventArgs)
                Try
                    Me.Validate()
                Catch ex As Exception
                    cLog.Write(ex, eVerboseLevel.Detailed, "NumericUpDownWrapper:OnValidate")
                End Try
            End Sub

#End Region ' NumericUpDown events

#Region " Style guide events "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Event handler, invoked when the Style Guide has been modified.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)

                If (changeType And cStyleGuide.eChangeType.NumberFormatting) = 0 Then Return

                Dim objValueType As Type = Me.m_provider.ValueType

                ' Set number of decimal places for appropriate data types
                If objValueType Is GetType(Single) Or objValueType Is GetType(Double) Then
                    Me.m_ud.DecimalPlaces = Me.m_sg.NumDigits
                Else
                    Me.m_ud.DecimalPlaces = 0
                End If

            End Sub

#End Region ' Style guide events

#Region " Internals "

            Private m_bInUpdate As Boolean = False
            Private m_decLatValidated As Decimal = -9999D

            Private Sub Validate()

                ' JS 12Sep17: Prevent loss of focus/validation/message/focus/loss of focus loops
                ' This can only occur on control initialization, where values in the model contain 
                ' impossible values. Later changes will be constrained by the min and max set in 
                ' the Control according to variable metadata.
                ' Values can only be disallowed for a handful parameters such as machine-specific 
                ' number of threads, or for values that have been changed in the database.
                If (Me.m_decLatValidated = Me.m_ud.Value) Then Return
                Me.m_decLatValidated = Me.m_ud.Value

                If (Me.m_bInUpdate) Then Return
                Me.m_bInUpdate = True

                ' Update internal value
                If (CDec(Me.m_provider.Value) <> Me.m_ud.Value) Then
                    Me.m_provider.Value = Me.m_ud.Value
                    Me.UpdateContent(Properties.cProperty.eChangeFlags.All)
                End If

                Me.m_bInUpdate = False

            End Sub

#End Region ' Internals

        End Class

#End Region ' Class NumericUpDownWrapper

#Region " Class ComboBoxWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, invoked when the Combo Box selected index has changed to
        ''' update the value into the parent <see cref="cEwEFormatProvider">cEwEFormatProvider</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cComboBoxWrapper
            Implements IControlWrapper

#Region " Private variables "

            ''' <summary>The wrapped combo box.</summary>
            Private m_cmb As ComboBox = Nothing
            ''' <summary></summary>
            Private m_provider As cEwEFormatProvider = Nothing
            Private m_tValue As Type = Nothing
            ''' <summary>Optional combo box items.</summary>
            Private m_lItems As New List(Of Object)

            Private m_md As cVariableMetaData = Nothing
            Private m_formatter As ITypeFormatter = Nothing

            Private m_bIsCoreIOCollection As Boolean = False

#End Region ' Private variables 

#Region " Implementation "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the UI context for this wrapper.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property UIContext() As cUIContext _
                Implements IUIElement.UIContext

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="ctrl"></param>
            ''' <param name="provider"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Public Function Wrap(ByVal ctrl As Control, _
                                 ByVal provider As cEwEFormatProvider, _
                                 Optional ByVal aItems() As Object = Nothing, _
                                 Optional ByVal metadata As EwECore.cVariableMetaData = Nothing, _
                                 Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean _
                Implements IControlWrapper.Wrap

                Dim bSucces As Boolean = True

                ' Sanity checks
                Debug.Assert(ctrl IsNot Nothing)
                Debug.Assert(provider IsNot Nothing)

                Try
                    ' Store ref to combo box
                    Me.m_cmb = DirectCast(ctrl, ComboBox)

                    ' Store refs
                    Me.m_provider = provider
                    Me.m_md = metadata
                    Me.m_formatter = formatter

                    ' Add handlers
                    AddHandler Me.m_cmb.SelectedIndexChanged, AddressOf OnComboBoxValueChanged
                    AddHandler Me.m_cmb.TextChanged, AddressOf OnComboBoxValueChanged
                    ' Beware: This will fire off the Format event as soon as the handler is created! Be ready...
                    AddHandler Me.m_cmb.Format, AddressOf OnComboBoxFormat

                    ' Populate combo
                    If aItems IsNot Nothing Then
                        ' Eradicate content
                        Me.Items = aItems
                    End If

                Catch e As Exception
                    ' Throw dev. time error
                    Debug.Assert(False, cStringUtils.Localize("Failed to wrap combo box {0}", ctrl.ToString()))
                    ' Report failure
                    bSucces = False
                End Try

                Return bSucces

            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Release the wrapped control.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub Release() _
                    Implements IControlWrapper.Release

                If (Me.m_cmb IsNot Nothing) Then
                    RemoveHandler Me.m_cmb.SelectedIndexChanged, AddressOf OnComboBoxValueChanged
                    RemoveHandler Me.m_cmb.TextChanged, AddressOf OnComboBoxValueChanged
                    RemoveHandler Me.m_cmb.Format, AddressOf OnComboBoxFormat
                    Me.m_cmb = Nothing
                End If

            End Sub

            Private m_bInUpdate As Boolean = False

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Update value and display style of the combo box.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags) _
                Implements IControlWrapper.UpdateContent

                If (Me.m_bInUpdate) Then Return

                Dim objValue As Object = Me.m_provider.Value
                Dim sg As cStyleGuide = Me.UIContext.StyleGuide
                Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                Dim bEditable As Boolean = ((style And cStyleGuide.eStyleFlags.NotEditable) = 0)

                ' ToDo: apply metadata
                Me.m_bInUpdate = True

                If (cf And Properties.cProperty.eChangeFlags.Value) > 0 Then

                    ' Sanity checks
                    If (objValue IsNot Nothing) Then
                        ' Update control
                        ' - Set selection state
                        Try
                            Me.SelectItem(objValue)
                        Catch ex As Exception

                        End Try
                    End If
                End If

                If (cf And Properties.cProperty.eChangeFlags.CoreStatus) > 0 Then

                    ' - Set colours
                    sg.GetStyleColors(style, Me.m_cmb.ForeColor, Me.m_cmb.BackColor)
                    ' - Set enabled state
                    Me.m_cmb.Enabled = bEditable

                End If

                Me.m_bInUpdate = False

            End Sub

            Private Sub SelectItem(ByVal objValue As Object)
                Dim iValue As Integer = -1

                ' Collection item case
                If Me.m_bIsCoreIOCollection Then
                    Dim iIndex As Integer = CInt(objValue)
                    Dim iNull As Integer = -1

                    For iItem As Integer = 0 To Me.m_cmb.Items.Count - 1
                        If (TypeOf Me.m_cmb.Items(iItem) Is ICoreInterface) Then
                            If (iIndex = DirectCast(Me.m_cmb.Items(iItem), ICoreInterface).Index) Then
                                iValue = iItem : Exit For
                            End If
                        Else
                            iNull = iItem
                        End If
                    Next
                    If (iValue = -1) Then iValue = iNull
                Else
                    ' Other data case
                    If (Not Me.m_lItems.Contains(objValue) And (Me.m_formatter Is Nothing)) Then
                        Me.m_lItems.Add(objValue)
                        Me.m_lItems.Sort()
                        ' Hmm
                        Me.Items = Me.Items
                    End If

                    ' Special enum resolve case
                    If (Me.m_provider.ValueType Is GetType(Integer)) Then
                        Dim objItem As Object = Nothing
                        For iItem As Integer = 0 To Me.m_cmb.Items.Count - 1
                            objItem = Me.m_cmb.Items(iItem)
                            If (TypeOf objValue Is Integer) Then
                                If (CInt(objValue) = CInt(objItem)) Then
                                    iValue = iItem : Exit For
                                End If
                            ElseIf (TypeOf objValue Is String) Then
                                If (String.Compare(CStr(objValue), CStr(objItem), False) = 0) Then
                                    iValue = iItem : Exit For
                                End If
                            Else
                                If Convert.Equals(objItem, objValue) Then
                                    iValue = iItem : Exit For
                                End If
                            End If
                        Next
                    Else
                        iValue = Me.m_lItems.IndexOf(objValue)
                    End If
                End If

                ' Truncate
                Me.m_cmb.SelectedIndex = Math.Max(-1, Math.Min(Me.m_cmb.Items.Count - 1, iValue))

            End Sub

            Private Function SelectedIndex() As Integer
                Dim iIndex As Integer = cCore.NULL_VALUE
                Dim objItem As Object = Nothing

                If (Me.m_cmb.SelectedIndex >= 0) Then
                    objItem = Me.m_cmb.SelectedItem()
                    iIndex = Me.m_cmb.SelectedIndex
                    If (TypeOf objItem Is ICoreInterface) Then
                        iIndex = DirectCast(objItem, ICoreInterface).Index
                    End If
                End If
                Return iIndex
            End Function

            Public Property Items() As Object() Implements IControlWrapper.Items
                Get
                    Return Me.m_lItems.ToArray
                End Get
                Set(ByVal aItems As Object())

                    ' Eradicate content
                    Me.m_lItems.Clear()
                    Me.m_cmb.Items.Clear()

                    ' Populate if new items given
                    If (aItems IsNot Nothing) Then
                        Me.m_lItems.AddRange(aItems)
                        ' Populate
                        For iItem As Integer = 0 To aItems.Length - 1
                            If ((Me.m_provider.ValueType Is GetType(Integer)) And (TypeOf (aItems(iItem)) Is ICoreInterface)) Then
                                If Not Me.m_bIsCoreIOCollection Then
                                    Me.m_bIsCoreIOCollection = True
                                    Me.m_formatter = New cCoreInterfaceFormatter()
                                End If
                            End If
                            Me.m_cmb.Items.Add(aItems(iItem))
                        Next
                    End If

                    ' Done
                    Me.UpdateContent(Properties.cProperty.eChangeFlags.All)
                End Set
            End Property

            Public ReadOnly Property Control As System.Windows.Forms.Control Implements IControlWrapper.Control
                Get
                    Return Me.m_cmb
                End Get
            End Property

            Public ReadOnly Property Metadata As EwECore.cVariableMetaData Implements IControlWrapper.Metadata
                Get
                    Return Me.m_md
                End Get
            End Property

#End Region ' Implementation 

#Region " ComboBox events "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Event handler, invoked when the Combo box selection or text have changed. 
            ''' This will pass the combo box selection to the parent <see cref="cEwEFormatProvider"/>.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub OnComboBoxValueChanged(ByVal sender As Object, ByVal e As System.EventArgs)

                If (Me.m_bInUpdate) Then Return
                Me.m_bInUpdate = True

                ' Update internal value
                If (Me.m_provider.ValueType Is GetType(Integer)) Then
                    ' #Integer? Set index
                    Me.m_provider.Value = Me.SelectedIndex()
                ElseIf Me.m_provider.ValueType Is GetType(String) Then
                    ' #String? Set text
                    Me.m_provider.Value = Me.m_cmb.Text
                Else
                    ' #Try to do automatic magic, somehow
                    Try
                        Me.m_provider.Value = Convert.ChangeType(Me.m_cmb.SelectedItem, Me.m_provider.ValueType)
                    Catch ex As Exception
                        Debug.Assert(False, "Unable to convert value type")
                    End Try
                End If
                Me.m_bInUpdate = False

            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Event handler, invoked when the combo box requires formatting.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub OnComboBoxFormat(ByVal sender As Object, ByVal e As ListControlConvertEventArgs)

                If (Me.m_formatter Is Nothing) Then Return
                If (Not Me.m_formatter.GetDescribedType().IsAssignableFrom(e.ListItem.GetType())) Then Return

                e.Value = Me.m_formatter.GetDescriptor(e.ListItem, eDescriptorTypes.Name)

            End Sub

#End Region ' ComboBox events

        End Class

#End Region ' Class ComboBoxWrapper

#Region " Class CheckboxWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cCheckboxWrapper
            Implements IControlWrapper

#Region " Private variables "

            ''' <summary>The wrapped check box.</summary>
            Private m_cb As CheckBox = Nothing
            ''' <summary></summary>
            Private m_provider As cEwEFormatProvider = Nothing

            Private m_md As cVariableMetaData = Nothing

#End Region ' Private variables 

#Region " Implementation "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the UI context for this wrapper.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property UIContext() As cUIContext _
                Implements IUIElement.UIContext

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="ctrl"></param>
            ''' <param name="provider"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Public Function Wrap(ByVal ctrl As Control, _
                                ByVal provider As cEwEFormatProvider, _
                                Optional ByVal aItems() As Object = Nothing, _
                                Optional ByVal metadata As EwECore.cVariableMetaData = Nothing, _
                                Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean _
                Implements IControlWrapper.Wrap

                Dim bSucces As Boolean = True

                If Not (provider.ValueType Is GetType(Boolean)) Then
                    Debug.Assert(False, "Checkboxes should only wrap boolean values")
                    Return False
                End If

                Try
                    ' Store ref to Text box
                    Me.m_cb = DirectCast(ctrl, CheckBox)
                    AddHandler Me.m_cb.CheckedChanged, AddressOf OnControlValueChanged

                    Me.m_provider = provider
                    Me.m_md = metadata

                Catch e As Exception
                    ' Throw dev. time error
                    Debug.Assert(False, cStringUtils.Localize("Failed to wrap checkbox {0}", ctrl.ToString()))
                    ' Report failure
                    bSucces = False
                End Try

                Return bSucces

            End Function

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Release the wrapped control.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Sub Release() _
                    Implements IControlWrapper.Release

                If (Me.m_cb IsNot Nothing) Then
                    RemoveHandler Me.m_cb.CheckedChanged, AddressOf OnControlValueChanged
                    Me.m_cb = Nothing
                End If

            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Update value and display style of the check box.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags) _
                Implements IControlWrapper.UpdateContent

                Dim sg As cStyleGuide = Me.UIContext.StyleGuide
                Dim objValue As Object = Me.m_provider.Value
                Dim objValueType As Object = Me.m_provider.ValueType
                Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                Dim bEditable As Boolean = ((style And cStyleGuide.eStyleFlags.NotEditable) = 0)

                If (cf And Properties.cProperty.eChangeFlags.Value) > 0 Then
                    ' Sanity checks
                    If objValue Is Nothing Then Return

                    ' Update control
                    ' - Set checked state
                    Me.m_cb.Checked = CBool(objValue)
                End If

                If (cf And Properties.cProperty.eChangeFlags.CoreStatus) > 0 Then

                    ' - Set colours
                    ' *** Checkbox special: do not colour background on "OK" or "NotEditable" style
                    style = style And Not (cStyleGuide.eStyleFlags.OK Or cStyleGuide.eStyleFlags.NotEditable)
                    Me.m_cb.BackColor = SystemColors.Control
                    ' Fetch, boy
                    sg.GetStyleColors(style, Me.m_cb.ForeColor, Me.m_cb.BackColor)
                    ' - Set enabled state
                    Me.m_cb.Enabled = bEditable

                    ' Highlight border
                    If (style And cStyleGuide.eStyleFlags.Highlight) > 0 Then
                        Me.m_cb.BackColor = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                    End If

                End If

            End Sub

            Public Property Items() As Object() Implements IControlWrapper.Items
                Get
                    Return Nothing
                End Get
                Set(ByVal value As Object())
                End Set
            End Property

            Public ReadOnly Property Control As System.Windows.Forms.Control Implements IControlWrapper.Control
                Get
                    Return Me.m_cb
                End Get
            End Property

            Public ReadOnly Property Metadata As EwECore.cVariableMetaData Implements IControlWrapper.Metadata
                Get
                    Return Me.m_md
                End Get
            End Property

#End Region ' Implementation

#Region " Control events "

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Event handler, invoked when the Check Box state has changed. This will 
            ''' pass the check box check state back to the parent 
            ''' <see cref="cEwEFormatProvider">cEwEFormatProvider</see>.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Private Sub OnControlValueChanged(ByVal sender As Object, ByVal e As System.EventArgs)
                ' Update internal value
                Me.m_provider.Value = Me.m_cb.Checked
            End Sub

#End Region ' Control events

        End Class

#End Region ' Class CheckboxWrapper

#Region " Class LabelWrapper "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Class cLabelWrapper
            Implements IControlWrapper

#Region " Private variables "

            ''' <summary>The wrapped label control.</summary>
            Private m_lb As Label = Nothing
            ''' <summary>The EwEFormatProvider that implements value and colour
            ''' behaviour onto the text box.</summary>
            Private m_provider As cEwEFormatProvider = Nothing

            Private m_md As cVariableMetaData = Nothing

#End Region ' Private variables 

#Region " Implementation "

            ''' ---------------------------------------------------------------
            ''' <summary>
            ''' Get/set the UI context for this wrapper.
            ''' </summary>
            ''' ---------------------------------------------------------------
            Public Property UIContext() As cUIContext _
                Implements IUIElement.UIContext

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="ctrl"></param>
            ''' <param name="provider"></param>
            ''' <returns></returns>
            ''' -----------------------------------------------------------------------
            Public Function Wrap(ByVal ctrl As Control, _
                                ByVal provider As cEwEFormatProvider, _
                                Optional ByVal aItems() As Object = Nothing, _
                                Optional ByVal metadata As EwECore.cVariableMetaData = Nothing, _
                                Optional ByVal formatter As ITypeFormatter = Nothing) As Boolean _
                Implements IControlWrapper.Wrap

                Dim bSucces As Boolean = True

                Try
                    ' Store ref to Text box
                    Me.m_lb = DirectCast(ctrl, Label)

                    Me.m_provider = provider
                    Me.m_md = metadata

                Catch e As Exception
                    ' Throw dev. time error
                    Debug.Assert(False, "Failed to wrap label")
                    ' Report failure
                    bSucces = False
                End Try

                Return bSucces

            End Function

            Public Sub Release() _
                    Implements IControlWrapper.Release

                Me.m_lb = Nothing

            End Sub

            ''' -----------------------------------------------------------------------
            ''' <summary>
            ''' Update value and display style of the label.
            ''' </summary>
            ''' -----------------------------------------------------------------------
            Public Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags) _
                Implements IControlWrapper.UpdateContent

                Dim sg As cStyleGuide = Me.UIContext.StyleGuide
                Dim objValue As Object = Me.m_provider.Value
                Dim objValueType As Object = Me.m_provider.ValueType
                Dim style As cStyleGuide.eStyleFlags = Me.m_provider.Style
                Dim strText As String = ""

                If (cf And Properties.cProperty.eChangeFlags.Value) > 0 Then

                    ' Sanity checks
                    If objValue Is Nothing Then Return

                    ' Get default value
                    strText = objValue.ToString()

                    ' Interpret as single?
                    If objValueType Is GetType(Single) Then
                        ' #Yes: apply format
                        strText = sg.FormatNumber(CSng(objValue), style)
                    End If

                    ' Interpret as double?
                    If objValueType Is GetType(Double) Then
                        ' #Yes: apply format
                        strText = sg.FormatNumber(CDbl(objValue), style)
                    End If

                    ' Update text box
                    ' - Set text
                    Me.m_lb.Text = strText

                End If

                If (cf And Properties.cProperty.eChangeFlags.CoreStatus) > 0 Then

                    ' - Set colours
                    sg.GetStyleColors(style And Not cStyleGuide.eStyleFlags.OK, Me.m_lb.ForeColor, Me.m_lb.BackColor)

                    ' Highlight border
                    If (style And cStyleGuide.eStyleFlags.Highlight) > 0 Then
                        Me.m_lb.BackColor = sg.ApplicationColor(cStyleGuide.eApplicationColorType.HIGHLIGHT)
                    End If

                End If

            End Sub

            Public Property Items() As Object() Implements IControlWrapper.Items
                Get
                    Return Nothing
                End Get
                Set(ByVal value As Object())
                End Set
            End Property

            Public ReadOnly Property Control As System.Windows.Forms.Control Implements IControlWrapper.Control
                Get
                    Return Me.m_lb
                End Get
            End Property

            Public ReadOnly Property Metadata As EwECore.cVariableMetaData Implements IControlWrapper.Metadata
                Get
                    Return Me.m_md
                End Get
            End Property

#End Region ' Implementation

        End Class

#End Region ' Class LabelWrapper

#End Region ' Control Wrappers

#End Region ' Private helper classes

#Region " Private vars "

        ' ToDo_JS: rework to contain the value in a cValue object, with a real validator

        ''' <summary>Value of the control.</summary>
        Private m_objValue As Object = Nothing
        ''' <summary><see cref="Type">Type</see> of the Value.</summary>
        Private m_tValue As Type = Nothing
        ''' <summary>EwE <see cref="cStyleGuide.eStyleFlags">Style</see> of the control.</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK
        ''' <summary>The wrapper that interacts with the control</summary>
        Private m_ctrlWrapper As IControlWrapper = Nothing
        ''' <summary>The wrapped control</summary>
        Protected m_ctrl As Control = Nothing

#End Region ' Private vars

#Region " Constructor "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a format provider for a control.
        ''' </summary>
        ''' <param name="uic">The UI context to use.</param>
        ''' <param name="ctrl">The control to wrap.</param>
        ''' <param name="metadata">Optional metadata to limit value ranges in the
        ''' control.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal ctrl As Control, _
                       ByVal tValue As Type, _
                       Optional ByVal aItems As Object() = Nothing, _
                       Optional ByVal metadata As cVariableMetaData = Nothing,
                       Optional ByVal formatter As ITypeFormatter = Nothing)

            ' Sanity checks
            Debug.Assert(uic IsNot Nothing)
            Debug.Assert(ctrl IsNot Nothing)

            ' Store value type
            Me.m_tValue = tValue
            ' Get wrapper
            Me.m_ctrlWrapper = cControlWrapperFactory.GetControlWrapper(uic, ctrl, Me, aItems, metadata, formatter)
            Me.m_ctrl = ctrl

            ' Cannot be
            Debug.Assert(Me.m_ctrlWrapper IsNot Nothing)

            ' Connect to style guide
            Me.UIContext = uic
            ' Respond to styleguide changes
            AddHandler Me.UIContext.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
            ' Respond to control closure events
            AddHandler Me.m_ctrlWrapper.Control.Disposed, AddressOf OnControlDisposed

            If (TypeOf (Me.m_ctrl) Is Control) Then
                AddHandler DirectCast(Me.m_ctrl, Control).Enter, AddressOf OnGotFocus
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrl"></param>
        ''' <param name="tValue"></param>
        ''' <param name="metadata"></param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal ctrl As Control, _
                       ByVal tValue As Type, _
                       ByVal metadata As cVariableMetaData)
            Me.New(uic, ctrl, tValue, Nothing, metadata)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a format provider for a control, deriving its value from an 
        ''' enumerated type.
        ''' </summary>
        ''' <param name="uic">The UI context to use.</param>
        ''' <param name="ctrl">The control to wrap.</param>
        ''' <param name="formatter"><see cref="IFormatProvider"/> that provides
        ''' a range of values to display in the control.</param>
        ''' <param name="metadata">Optional metadata to limit value ranges in the
        ''' control.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal ctrl As Control, _
                       ByVal formatter As ITypeFormatter, _
                       ByVal metadata As cVariableMetaData)
            Me.New(uic, ctrl, GetType(Integer), ExtractEnumValues(formatter), Nothing, formatter)
        End Sub

#End Region ' Constructor

#Region " Release "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Release the format provider from the wrapped control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Sub Release()

            If (Me.m_ctrlWrapper IsNot Nothing) Then
                RemoveHandler Me.m_ctrlWrapper.Control.Disposed, AddressOf OnControlDisposed
                Me.m_ctrlWrapper.Release()
                Me.m_ctrlWrapper = Nothing
            End If

            Me.m_ctrl = Nothing

            If Me.UIContext IsNot Nothing Then
                RemoveHandler Me.UIContext.StyleGuide.StyleGuideChanged, AddressOf OnStyleGuideChanged
                Me.UIContext = Nothing
            End If

        End Sub

#End Region ' Release

#Region " Interface "

        Public Property UIContext() As cUIContext Implements IUIElement.UIContext

#End Region ' Interface

#Region " Value "

        ''' -------------------------------------------------------------------
        ''' <summary>Event to notify that a value has changed.</summary>
        ''' <param name="sender">The format provider that sent the event.</param>
        ''' -------------------------------------------------------------------
        Public Event OnValueChanged(ByVal sender As Object, args As EventArgs)

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value of the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Value() As Object
            Get
                Return Me.m_objValue
            End Get
            Set(ByVal objValue As Object)

                ' ToDo_JS: rework to use cValue validation
                ' ToDo_JS: set style to match validation styles

                Dim objValueConverted As Object = Nothing

                Try
                    ' First convert value
                    If Me.m_tValue Is GetType(String) Then
                        objValueConverted = CStr(objValue)
                    ElseIf Me.m_tValue Is GetType(Integer) Then
                        objValueConverted = Convert.ToInt32(objValue)
                    ElseIf Me.m_tValue Is GetType(Single) Then
                        objValueConverted = Convert.ToSingle(objValue)
                    ElseIf Me.m_tValue Is GetType(Double) Then
                        objValueConverted = Convert.ToDouble(objValue)
                    Else
                        objValueConverted = objValue
                    End If

                Catch ex As Exception
                    ' Decline!
                    Return
                End Try

                ' Check for changes
                If Me.m_objValue IsNot Nothing Then
                    If Me.m_objValue.Equals(objValueConverted) Then
                        ' No changes: do not set value
                        Return
                    End If
                End If

                ' Set value
                Me.m_objValue = objValueConverted

                ' Update
                Me.UpdateContent(Properties.cProperty.eChangeFlags.Value)
                Me.RaiseChangeEvent()

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the <see cref="cStyleGuide.eStyleFlags">Style</see> to reflect 
        ''' in the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overridable Property Style() As cStyleGuide.eStyleFlags
            Get
                Return Me.m_style
            End Get
            Set(ByVal s As cStyleGuide.eStyleFlags)
                ' Store style
                Me.m_style = s
                ' Update
                Me.UpdateContent(Properties.cProperty.eChangeFlags.CoreStatus)
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get the type of the value in the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Protected ReadOnly Property ValueType() As Type
            Get
                Return Me.m_tValue
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get or set the lsit of items to display in the control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Property Items() As Object()
            Get
                Return Me.m_ctrlWrapper.Items
            End Get
            Set(ByVal aItems As Object())
                Me.m_ctrlWrapper.Items = aItems
            End Set
        End Property

        Public Property Enabled() As Boolean
            Get
                Return ((Me.m_style And cStyleGuide.eStyleFlags.NotEditable) <> cStyleGuide.eStyleFlags.NotEditable)
            End Get
            Set(ByVal value As Boolean)
                If value = True Then
                    Me.Style = Me.Style And (Not cStyleGuide.eStyleFlags.NotEditable)
                Else
                    Me.Style = Me.Style Or cStyleGuide.eStyleFlags.NotEditable
                End If
            End Set
        End Property

#End Region ' Value

#Region " Updates "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the wrapped control receives focus. Handled to fire 
        ''' an application-wide <see cref="cPropertySelectionCommand">PropertySelectionCommand</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overridable Sub OnGotFocus(ByVal sender As Object, ByVal e As System.EventArgs)

            If (Me.UIContext Is Nothing) Then Return

            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            If (cmdh Is Nothing) Then Return

            Dim dsc As cPropertySelectionCommand = DirectCast(cmdh.GetCommand(cPropertySelectionCommand.COMMAND_NAME), cPropertySelectionCommand)
            If (dsc Is Nothing) Then Return

            dsc.Invoke()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' cStyleGuide change event handler; makes sure cells are redrawn
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub OnStyleGuideChanged(ByVal changeType As cStyleGuide.eChangeType)
            Me.UpdateContent(Properties.cProperty.eChangeFlags.All)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, responded to the event that a control was disposed while
        ''' the format provider was still active. This will cause memory leaks.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Private Sub OnControlDisposed(ByVal sender As Object, ByVal args As EventArgs)

            ' Pre
            Debug.Assert(Me.m_ctrlWrapper IsNot Nothing)
            Debug.Assert(Me.m_ctrlWrapper.Control IsNot Nothing)

            Console.WriteLine("Format provider for control " & Me.m_ctrlWrapper.Control.Name & " auto-releasing")
            Me.Release()

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Update the attached control
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub UpdateContent(cf As ScientificInterfaceShared.Properties.cProperty.eChangeFlags)
            If Me.m_ctrlWrapper IsNot Nothing Then
                Me.m_ctrlWrapper.UpdateContent(cf)
            End If
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Raises the <see cref="OnValueChanged">OnValueChanged</see> ewvent.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Sub RaiseChangeEvent()
            Try
                RaiseEvent OnValueChanged(Me, Nothing)
            Catch ex As Exception
                ' Wow
            End Try

        End Sub

#End Region ' Updates

#Region " Tag "

        Public Property Tag As Object

#End Region ' Tag

#Region " Internals "

        Protected Shared Function ExtractEnumValues(ByVal formatter As ITypeFormatter) As Object()

            Dim lItems As New List(Of Object)
            ' Get first generic argument
            Dim tEnum As Type = formatter.GetDescribedType
            ' IS an enum?
            If tEnum.IsEnum Then
                Try
                    For Each v As Object In [Enum].GetValues(tEnum)
                        lItems.Add(v)
                    Next
                Catch ex As Exception
                    ' Whoah!
                    Debug.Assert(False, ex.Message)
                End Try
            End If
            Return lItems.ToArray()

        End Function

#End Region ' Internals

        Public ReadOnly Property Wrapper As IControlWrapper
            Get
                Return Me.m_ctrlWrapper
            End Get
        End Property

    End Class

End Namespace
