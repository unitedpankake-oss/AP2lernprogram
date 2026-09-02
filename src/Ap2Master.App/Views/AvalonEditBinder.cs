using System.Windows;
using ICSharpCode.AvalonEdit;

namespace Ap2Master.App.Views;

/// <summary>
/// Two-way text binding for AvalonEdit, which has no native Text DP.
/// </summary>
public static class AvalonEditBinder
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text", typeof(string), typeof(AvalonEditBinder),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextEditor editor) return;
        var next = e.NewValue as string ?? "";
        if (editor.Text == next) return;
        editor.Text = next;
        if (editor.Tag as string == "bound") return;
        editor.Tag = "bound";
        editor.TextChanged += (_, _) => SetText(editor, editor.Text);
    }
}
