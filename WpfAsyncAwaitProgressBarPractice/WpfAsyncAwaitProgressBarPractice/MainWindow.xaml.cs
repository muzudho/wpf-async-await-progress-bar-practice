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
                // 1件 0.5 秒かかるとします
                Thread.Sleep(500);

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

            // 通り抜けます
        }

        /// <summary>
        /// [処理３ 同期（うまく行かない例）]ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BadSync3Button_Click(object sender, RoutedEventArgs e)
        {
            // では サブウィンドウを作ること自体を別スレッドで行えばどうでしょうか？

            var func = () =>
            {
                try
                {
                    // なんと、ウィンドウを作ること自体できません。 STA エラー
                    var subWindow = new ProgressBarWindowView();
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("例外が発生しました", "[処理３ 同期（うまく行かない例）]ボタン押下時", MessageBoxButton.OK, MessageBoxImage.Error);
                    return $"処理３ 未完了";
                }

                // ここは通りません
                return $"処理３ 完了";
            };

            // 待機しません
            Task.Run(func);

            // 通り抜けます
        }

        /// <summary>
        /// [処理４ 同期（動く例）]ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BadSync4Button_Click(object sender, RoutedEventArgs e)
        {
            // サブウィンドウ（プログレスバーが置いてあります）を表示します
            var subWindow = new ProgressBarWindowView();
            subWindow.textBlock1.Text = "プログレスバーが動くようになりました";

            // サブウィンドウを表示します
            subWindow.Show();

            // Progressオブジェクトを使うのが工夫です
            IProgress<int> progress = new Progress<int>((percentage) =>
            {
                // このコードブロックは、 BadSync4Button_Click メソッドと同じスレッドです。
                // あとで progress.Report(parcentage); と呼び出すことで実行されます
                subWindow.progressBar1.Value = percentage;

                // シングルスレッドなので、ちゃんとプログラミングしていれば 100 になるはずです
                if (percentage == 100)
                {
                    // サブウィンドウを閉じます
                    subWindow.Close();
                }
            });

            // 中で await を使っているので、 async 修飾子を付ける必要があります
            var func = async () =>
            {
                // このコードブロックの中では UI（ウィンドウとか）に直接アクセスしないのが工夫です

                const int kTotal = 10;
                for (int i = 0; i < kTotal; i++)
                {
                    // 1件 0.5 秒かかるとします
                    await Task.Delay(500);

                    var parcentage = (i + 1) * 100 / kTotal;

                    // progress オブジェクトを介します
                    progress.Report(parcentage);
                }

                return $"処理２ 完了";
            };

            // 待機しません
            Task.Run(func);

            // 通り抜けます
        }
    }
}
