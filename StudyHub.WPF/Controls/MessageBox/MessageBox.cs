namespace StudyHub.WPF.Controls;

public class MessageBox : Wpf.Ui.Controls.MessageBox {
    public void ClickClose() {
        base.OnButtonClick(Wpf.Ui.Controls.MessageBoxButton.Close);
    }

    public void ClickPrimary() {
        base.OnButtonClick(Wpf.Ui.Controls.MessageBoxButton.Primary);
    }

    public void ClickSecondary() {
        base.OnButtonClick(Wpf.Ui.Controls.MessageBoxButton.Secondary);
    }
}
