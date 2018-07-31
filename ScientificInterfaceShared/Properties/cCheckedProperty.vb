'==============================================================================
'
' $Log: cCheckedProperty.vb,v $
' Revision 1.1  2009/04/02 13:22:08  jeroens
' Separated derived classes out of cProperty.vb
'
'==============================================================================

Option Strict On
Imports EwECore
Imports EwECore.ValueWrapper
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

Namespace Properties

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Specialized cProperty class, designed to behave like a check box or radio button
    ''' by observing a particular value in another cProperty.
    ''' </summary>
    ''' <remarks>
    ''' JS 02apr09: this class has not yet been used and has hence been commented out
    ''' from the project.
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Class cCheckedProperty
        Inherits cStringProperty

        ''' <summary>A property to observe.</summary>
        Private m_prop As cProperty = Nothing
        ''' <summary>The value of m_prop that this property represents.</summary>
        Private m_value As Object = Nothing

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes the property.
        ''' </summary>
        ''' <param name="prop">The <see cref="cProperty">Property</see> to observe.</param>
        ''' <param name="value">The value of <paramref name="prop">prop</paramref> that 
        ''' this instance represents.</param>
        ''' -----------------------------------------------------------------------
        Public Sub New(ByVal prop As cProperty, ByVal value As Object)
            MyBase.New("")

            ' Sanity check
            Debug.Assert(prop IsNot Nothing)

            Me.m_prop = prop
            Me.m_value = CObj(value)

            ' Listen for property changes
            AddHandler m_prop.PropertyChanged, AddressOf OnPropertyChanged
            Me.OnPropertyChanged(m_prop, eChangeFlags.All)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, called when the observed property has changed.
        ''' </summary>
        ''' <param name="prop">The changed <see cref="cProperty">property</see>.</param>
        ''' <param name="changeFlags">Bitwise <see cref="cProperty.eChangeFlags">flag</see> 
        ''' describing what aspect of the property changed.</param>
        ''' -----------------------------------------------------------------------
        Private Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)

            Dim bIsMyValue As Boolean = False
            Dim value As Object = prop.GetValue()
            Dim sf As StyleGuide.eStyleFlags = prop.GetStyle()
            Dim strValue As String = ""

            ' Value has not changed?
            If (changeFlags And (eChangeFlags.Value Or eChangeFlags.CoreStatus)) = 0 Then
                ' #No relevant changes: abort
                Return
            End If

            ' Check property value against instance value
            If prop.IsValue(Me.m_value) Then
                strValue = "X"
                sf = (sf Or StyleGuide.eStyleFlags.Checked)
            Else
                strValue = ""
            End If

            Me.Value = strValue
            Me.FireChangeNotification(eChangeFlags.Value)

            Me.SetStyle(sf, TriState.UseDefault, eBitSetMode.All)

        End Sub

        ''' -----------------------------------------------------------------------
        ''' <summary>
        ''' User edit entry point. An edit operation resulted in a change of the value in the cell. This will
        ''' set the value of the underlying property to the value observed by this instance.
        ''' </summary>
        ''' <param name="newValue">The edited value.</param>
        ''' <param name="notify">States whether a notification must be broadcasted.</param>
        ''' <returns></returns>
        ''' -----------------------------------------------------------------------
        Public Overrides Function SetValue(ByVal newValue As Object, Optional ByVal notify As TriState = TriState.UseDefault) As Boolean

            Dim bResult As Boolean = True

            ' Is any value set?
            If newValue IsNot Nothing Then
                ' #Yes: Update the underlying property with our instance value.
                bResult = Me.m_prop.SetValue(Me.m_value, TriState.UseDefault)
            End If
            Return bResult

        End Function

    End Class

End Namespace
