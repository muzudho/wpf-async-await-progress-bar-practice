namespace WpfAsyncAwaitProgressBarPractice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

                var percentage = (i + 1) * 100 / kTotal;
                progressBar.Value = percentage;
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
                catch (InvalidOperationException)
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
                catch (InvalidOperationException)
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
        private void Sync4Button_Click(object sender, RoutedEventArgs e)
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

                // シングルスレッドなので、ちゃんとプログラミングしていれば 100 になります
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

                    var percentage = (i + 1) * 100 / kTotal;

                    // progress オブジェクトを介して UI を更新します
                    progress.Report(percentage);
                }

                return $"処理２ 完了";
            };

            // 待機しません
            Task.Run(func);

            // 通り抜けます
        }

        /// <summary>
        /// TODO (疑問点) グローバル変数でいいのか 分かりません
        /// </summary>
        private static int finishedCount;
        private static int totalCount;

        /// <summary>
        /// [処理５ 非同期]ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Async5Button_Click(object sender, RoutedEventArgs e)
        {
            // サブウィンドウ（プログレスバーが置いてあります）を表示します
            var subWindow = new ProgressBarWindowView();
            subWindow.textBlock1.Text = "並行して処理しています";

            // サブウィンドウを表示します
            subWindow.Show();

            // Progressオブジェクトを使うのが工夫です
            IProgress<int> progress = new Progress<int>((percentage) =>
            {
                // このコードブロックは、 BadSync4Button_Click メソッドと同じスレッドです。
                // あとで progress.Report(parcentage); と呼び出すことで実行されます
                subWindow.progressBar1.Value = percentage;

                // マルチスレッドを ちゃんと気を使ったプログラミングをしていれば 100 になります
                if (percentage == 100)
                {
                    // サブウィンドウを閉じます
                    subWindow.Close();
                }
            });

            var asyncFuncList = new List<Task<string>>();
            // 処理が複数個あるとします
            asyncFuncList.Add(DoWorkAsync(progress, "仕事1（普通）", 3000));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事2（普通）", 2500));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事3（軽い）", 2000));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事4（軽い）", 1500));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事5（軽い）", 1000));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事6（普通）", 3500));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事7（重い）", 4000));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事8（重い）", 4500));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事9（重い）", 5000));
            asyncFuncList.Add(DoWorkAsync(progress, "仕事10（重い）", 5500));

            // カウントをリセットします
            MainWindow.finishedCount = 0;
            MainWindow.totalCount = asyncFuncList.Count;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // 全てのタスクが終了するまで待機します
            // 結果の戻り値は、引数で渡したリストの順が保たれます
            var resultList = await Task.WhenAll(asyncFuncList);
            // TODO (疑問点) このメソッドを抜けなくても UIが更新されるのは何故か？

            stopwatch.Stop();

            // （累計ではなく）一番大きな数である 5秒程度で完了することに注目してください
            var message = String.Join("/", resultList);
            MessageBox.Show($"完了（{stopwatch.Elapsed}）: {message}", "[処理５ 非同期]ボタン押下時", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task<string> DoWorkAsync(IProgress<int> progress, string taskName, int milliseconds)
        {
            // このコードブロックの中では UI（ウィンドウとか）に直接アクセスしてはいけません

            await Task.Delay(milliseconds);

            // 処理が完了しました
            // atomic に 1 増やします
            Interlocked.Increment(ref MainWindow.finishedCount);

            var percentage = MainWindow.finishedCount * 100 / MainWindow.totalCount;
            progress.Report(percentage);

            return taskName;
        }
    }
}
