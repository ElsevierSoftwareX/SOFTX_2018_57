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
Imports EwEUtils.Core
Imports EwEUtils.Utilities
Imports ScientificInterfaceShared.Commands
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Controls

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <see cref="cEwEFormatProvider">cEwEFormatProvider</see> that is driven
    ''' by a <see cref="cProperty">cProperty</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Class cPropertyFormatProvider
        Inherits cEwEFormatProvider

        ''' <summary>Property that serves as data and style source.</summary>
        Private m_prop As cProperty = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrl"></param>
        ''' <param name="source"></param>
        ''' <param name="varName"></param>
        ''' <param name="sourceSec"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByVal ctrl As Control,
                       ByVal source As cCoreInputOutputBase,
                       ByVal varName As eVarNameFlags,
                       Optional ByVal sourceSec As cCoreInputOutputBase = Nothing)
            Me.New(uic, ctrl, uic.PropertyManager.GetProperty(source, varName, sourceSec))
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrl"></param>
        ''' <param name="source"></param>
        ''' <param name="varName"></param>
        ''' <param name="sourceSec"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal ctrl As Control, _
                       ByVal source As cCoreInputOutputBase, _
                       ByVal varName As eVarNameFlags, _
                       ByVal sourceSec As cCoreInputOutputBase, _
                       ByVal formatter As ITypeFormatter)
            Me.New(uic, ctrl, uic.PropertyManager.GetProperty(source, varName, sourceSec), formatter)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrl"></param>
        ''' <param name="prop"></param>
        ''' <param name="formatter">Formatter to obtain values from</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext, _
                       ByVal ctrl As Control, _
                       ByVal prop As cProperty, _
                       ByVal formatter As ITypeFormatter)
            Me.New(uic, ctrl, prop, DirectCast(ExtractEnumValues(formatter), Object()), formatter)
        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrl"></param>
        ''' <param name="prop"></param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal uic As cUIContext,
                       ByVal ctrl As Control,
                       ByVal prop As cProperty,
                       Optional ByVal aItems As Object() = Nothing,
                       Optional ByVal formatter As ITypeFormatter = Nothing)

            MyBase.New(uic, ctrl, prop.GetValueType(), aItems, prop.GetVariableMetadata(), formatter)

            ' Store relevant bits
            Me.m_prop = prop
            AddHandler Me.m_prop.PropertyChanged, AddressOf OnPropertyChanged

            ' Fire change event manually to immediately show the property value
            Me.OnPropertyChanged(Me.m_prop, cProperty.eChangeFlags.All)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Release the format provider from the wrapped control.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Release()

            If Me.m_prop IsNot Nothing Then
                RemoveHandler Me.m_prop.PropertyChanged, AddressOf OnPropertyChanged
                Me.m_prop = Nothing
            End If

            If Me.m_ctrl IsNot Nothing Then
                cToolTipShared.GetInstance().SetToolTip(Me.m_ctrl, "")
            End If

            MyBase.Release()

        End Sub

#Region " Data "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the style to reflect in the TextBox, overriding the style
        ''' dictated by the underlying cProperty.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Dim eStyle As cStyleGuide.eStyleFlags = MyBase.Style()
                If (eStyle <> cStyleGuide.eStyleFlags.OK) Then Return eStyle
                If Me.m_prop Is Nothing Then Return Nothing
                Return Me.m_prop.GetStyle()
            End Get
            Set(ByVal eStyle As cStyleGuide.eStyleFlags)
                MyBase.Style = eStyle
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get/set the value maintained in the underlying Property.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property Value() As Object
            Get
                If Me.m_prop Is Nothing Then Return Nothing
                Return Me.m_prop.GetValue()
            End Get
            Set(ByVal value As Object)
                If Me.m_prop Is Nothing Then Return
                If Me.m_prop.SetValue(value, TriState.UseDefault) Then
                    ' Ok, this is odd. Proper code should consult the underlying 
                    ' property directly
                    Me.RaiseChangeEvent()
                End If
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Sub Refresh()
            Me.m_prop.Refresh()
        End Sub

#End Region ' Data

#Region " Local events "

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, invoked when the source Property <see cref="cProperty.PropertyChanged">changes</see>.
        ''' </summary>
        ''' <param name="prop">The property that changed.</param>
        ''' <param name="changeFlags">The type of change.</param>
        ''' -----------------------------------------------------------------------
        Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)

            ' Sanity check
            Debug.Assert(ReferenceEquals(prop, Me.m_prop))

            ' Update control
            If (changeFlags And (cProperty.eChangeFlags.CoreStatus Or cProperty.eChangeFlags.Value)) > 0 Then
                ' Get new content
                Me.UpdateContent(changeFlags)
            End If

            ' Update tooltip
            If ((changeFlags And cProperty.eChangeFlags.Remarks) > 0) Then
                If (TypeOf (Me.m_ctrl) Is Control) Then
                    cToolTipShared.GetInstance().SetToolTip(Me.m_ctrl, prop.GetRemark())
                End If
            End If

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the wrapped control receives focus. Handled to fire 
        ''' an application-wide <see cref="cPropertySelectionCommand">PropertySelectionCommand</see>.
        ''' </summary>
        ''' -----------------------------------------------------------------------
        Protected Overrides Sub OnGotFocus(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim cmdh As cCommandHandler = Me.UIContext.CommandHandler
            Dim dsc As cPropertySelectionCommand = DirectCast(cmdh.GetCommand(cPropertySelectionCommand.COMMAND_NAME), cPropertySelectionCommand)

            If dsc Is Nothing Then Return

            dsc.Invoke(Me.m_prop)
        End Sub

#End Region ' Local events 

    End Class ' cPropertyFormatProvider

End Namespace ' Controls
