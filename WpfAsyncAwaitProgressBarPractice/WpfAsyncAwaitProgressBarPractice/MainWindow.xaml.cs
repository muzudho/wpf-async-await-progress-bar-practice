namespace WpfAsyncAwaitProgressBarPractice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// [処理１ 同期（うまく行かない例）]ボタン押下時
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
            Do10Works(subWindow.progressBar1);

            // サブウィンドウを閉じます
            subWindow.Close();
        }

        private static void Do10Works(ProgressBar progressBar)
        {
            const int kTotal = 10;
            for (int i = 0; i < kTotal; i++)
            {
                // 1秒かかるとします
                Thread.Sleep(1000);

                progressBar.Value = i * 100 / kTotal;
            }
        }

        /// <summary>
        /// [処理２ 同期（うまく行かない例）]ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sync2Button_Click(object sender, RoutedEventArgs e)
        {
            // サブウィンドウ（プログレスバーが置いてあります）を表示します
            var subWindow = new ProgressBarWindowView();
            subWindow.textBlock1.Text = "このメッセージは表示されますが、\nプログレスバーにアクセスできません";

            // サブウィンドウを表示します
            subWindow.Show();

            // この Sync2Button_Click メソッドが終わらないと、
            // 別ウィンドウのプログレスバーを描画するということもできません

            var func = () =>
            {
                // そこで、プログレスバーの更新と、ウィンドウを閉じることを
                // 別スレッドで行います

                // ここで時間のかかる処理を行います（うまく動きません）
                // このスレッドは subWindowオブジェクト を持っていないので、（元のスレッドが持っている）
                // アクセスできません
                try
                {
                    Do10Works(subWindow.progressBar1);
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("例外が発生しました", "[処理２ 同期（うまく行かない例）]ボタン押下時", MessageBoxButton.OK, MessageBoxImage.Error);
                    return $"処理２ 未完了";
                }

                // ここは通りません

                // サブウィンドウを閉じます
                subWindow.Close();

                return $"処理２ 完了";
            };

            // 待機しません
            Task.Run(func);
        }
    }
}
