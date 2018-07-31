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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' <para>
    ''' Returns one of two operands, depending on the evaluation of a test operand.
    ''' </para>
    ''' <para>This operation behaves exactly like IIf, other than that the expression 
    ''' responds to live data changes of all three parameters.</para>
    ''' </summary>
    ''' <remarks>
    ''' This expression monitors its property for value changes, and will broadcast 
    ''' a change event if this occurs.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Public Class cConditionalOperation
        : Inherits cExpression

        ''' <summary>The boolean test operand that will determine the outcome of this operation.</summary>
        Private WithEvents m_opTest As cBooleanOperand = Nothing
        ''' <summary>The operand that will be returned when the expression evaluates to True.</summary>
        Private WithEvents m_opTrue As cExpression = Nothing
        ''' <summary>The operand that will be returned when the expression evaluates to False.</summary>
        Private WithEvents m_opFalse As cExpression = Nothing
        ''' <summary>Cached calcuated value</summary>
        Private m_sValue As Single = 0.0
        ''' <summary>Cached style</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new cConditionalOperation.
        ''' </summary>
        ''' <param name="opTest">The <see cref="cBooleanOperand">test operand</see>
        ''' that determines which operand will be returned.
        ''' </param>
        ''' <param name="opTrue">The operand that will be returned when the expression evaluates to True.</param>
        ''' <param name="opFalse">The operand that will be returned when the expression evaluates to False.</param>
        ''' <remarks>
        ''' <para>Evaluation of <paramref name="opTest">opTest</paramref> will 
        ''' result in the following:</para>
        ''' <list type="bullet">
        ''' <item>When True, <paramref name="opTrue">opTrue</paramref> will deliver the
        ''' value for this expression.</item>
        ''' <item>When False, <paramref name="opFalse">opFalse</paramref> will deliver the
        ''' value for this expression.</item>
        ''' </list>
        ''' </remarks>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal opTest As cBooleanOperand, ByVal opTrue As Object, ByVal opFalse As Object)
            ' Store bits
            Me.m_opTest = opTest
            Me.m_opTrue = cFormulaProperty.GetExpression(opTrue)
            Me.m_opFalse = cFormulaProperty.GetExpression(opFalse)
            ' Initialize contents
            Me.m_sValue = Me.CalcValue()
            Me.m_style = Me.CalcStyle()
            ' Start listening for operand changes
            AddHandler Me.m_opTest.OnValueChanged, AddressOf OnOperandValueChanged
            AddHandler Me.m_opTrue.OnValueChanged, AddressOf OnOperandValueChanged
            AddHandler Me.m_opFalse.OnValueChanged, AddressOf OnOperandValueChanged
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            ' Stop listening for operand changes
            RemoveHandler Me.m_opTest.OnValueChanged, AddressOf OnOperandValueChanged
            RemoveHandler Me.m_opTrue.OnValueChanged, AddressOf OnOperandValueChanged
            RemoveHandler Me.m_opFalse.OnValueChanged, AddressOf OnOperandValueChanged
            Me.m_opTest.Dispose()
            Me.m_opTest = Nothing
            Me.m_opTrue.Dispose()
            Me.m_opTrue = Nothing
            Me.m_opFalse.Dispose()
            Me.m_opFalse = Nothing
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetValue() As Single
            Return Me.m_sValue
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="cStyleGuide.eStyleFlags">style</see>
        ''' of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetStyle() As cStyleGuide.eStyleFlags
            Return Me.m_style
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Event handler, responds to operand change events by recalculating
        ''' the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Sub OnOperandValueChanged(ByVal exp As cExpression)

            ' Calc new value and style
            Dim sVal As Single = Me.CalcValue()
            Dim style As cStyleGuide.eStyleFlags = Me.CalcStyle()
            ' Changes?
            If ((sVal <> Me.m_sValue) Or (Me.m_style <> style)) Then
                ' #Yes: set new value and style
                Me.m_sValue = sVal
                Me.m_style = style

                ' Broadcast change notification
                FireChangeNotification()
            End If

        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcValue() As Single
            If Me.m_opTest.GetValue() <> CSng(False) Then
                Return Me.m_opTrue.GetValue()
            Else
                Return Me.m_opFalse.GetValue()
            End If
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the style of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcStyle() As cStyleGuide.eStyleFlags
            If CBool(Me.m_opTest.GetValue) Then
                Return Me.m_opTrue.GetStyle()
            Else
                Return Me.m_opFalse.GetStyle()
            End If
        End Function

    End Class

End Namespace
