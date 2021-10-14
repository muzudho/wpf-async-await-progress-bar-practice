namespace WpfAsyncAwaitProgressBarPractice
{
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using WpfAsyncAwaitProgressBarPractice.Views;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// [同期処理１（うまく行かない例）]ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BadSyncButton_Click(object sender, RoutedEventArgs e)
        {
            // サブウィンドウ（プログレスバーが置いてあります）を表示します
            var subWindow = new ProgressBarWindowView();
            subWindow.textBlock1.Text = "このメッセージは表示されません";

            // サブウィンドウを表示します
            subWindow.Show();

            // ここで時間のかかる処理を行います（うまく動きません）
            DoWork(subWindow.progressBar1);

            // サブウィンドウを閉じます
            subWindow.Close();
        }

        private static void DoWork(ProgressBar progressBar)
        {
            const int kTotal = 10;
            for (int i = 0; i < kTotal; i++)
            {
                // 1秒かかるとします
                Thread.Sleep(1000);

                progressBar.Value = i * 100 / kTotal;
            }
        }
    }
}
