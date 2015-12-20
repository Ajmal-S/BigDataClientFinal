using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.Distributions;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web.UI;
using System.Threading;

namespace BigDataClientFinal
{
    public partial class MainWindow : Window
    {
        public Dictionary<string, int> clusters = new Dictionary<string, int>();
        public double[] r1 = new double[120];
        public string debug="";
        public double[] r2 = new double[120];
        public List<string> inn1 = new List<string>();
        public List<string> inn2 = new List<string>();
        public int d1, d2;
        public string result = "";
        CancellationTokenSource cts;
        public struct inning
        {
            public int score;
            public string name;
            public Dictionary<string, int[]> bowlers;
            public Dictionary<string, int> batsman;
        };
        public inning in1 = new inning();
        public bool skip = false,skip2=false;
        public inning in2 = new inning();
        public Dictionary<int, List<double>>[] clusterPvP = new Dictionary<int, List<double>>[]
        {
            new Dictionary<int, List<double>>(),
            new Dictionary<int, List<double>>(),
            new Dictionary<int, List<double>>(),
            new Dictionary<int, List<double>>(),
            new Dictionary<int, List<double>>(),
            new Dictionary<int, List<double>>(),
        };
        public bool t1s, t2s;
        public int dist;
        public List<string> Team1 = new List<string>();
        public List<string> Team2 = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            initStart();
        }
        private void initStart()
        {
            playGrid.Visibility=Visibility.Hidden;
            ResultsGrid.Visibility = Visibility.Hidden;
            startGrid.Visibility = Visibility.Visible;
            distCombo1.SelectedIndex = 0;
            distCombo2.SelectedIndex = 0;
            t1s = t2s = false;
            dist = 0;
            t1selConf.Visibility = Visibility.Hidden;
            t2selConf.Visibility = Visibility.Hidden;
            //replace with path of Clusters.txt in your comp
            try
            {
                string dump = File.ReadAllText(@"e:\bigData\Clusters.txt");
                var l = dump.Split(';');
                for (int i = 0; i < l.Length; i++)
                {
                    string g = l[i];
                    g = g.Remove(0, 1);
                    g = g.Remove(g.Length - 1);
                    g = g.Replace(".txt", "");
                    l[i] = g;
                }
                for (int i = 0; i < l.Length; i++)
                {
                    var t = l[i].Split(',');
                    for (int j = 0; j < t.Length; j++)
                    {
                        clusters[t[j]] = i;
                    }
                }
                for (int i = 0; i < clusterPvP.Length; i++)
                {
                    //repalce e:\\bigData\ with wherever you store the probability files
                    string fn = @"e:\bigData\clp" + (i + 1) + ".txt";
                    string s = File.ReadAllText(fn);
                    var p = s.Split(';');
                    for (int k = 0; k < p.Length; k++)
                    {
                        var x = p[k].Split(':');
                        int key = Int32.Parse(x[0]);
                        var v1 = x[1].Split(',');
                        List<double> val = v1.Select(z => double.Parse(z)).ToList();
                        clusterPvP[i][key] = val;
                    }
                }
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }
        }
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

        }

        private void team2select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //replace text with default directory where you're keeping teams
            try
            {
                openFileDialog1.InitialDirectory = "e:\\bigData\\";
                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.ShowDialog();
                Team2 = File.ReadAllLines(openFileDialog1.FileName).ToList();
                t2selConf.Visibility = Visibility.Visible;
                in2.name = openFileDialog1.SafeFileName.Replace(".txt", "");
                in2.batsman = new Dictionary<string, int>();
                in2.bowlers = new Dictionary<string, int[]>();
                foreach (string n in Team2[1].Split(':')[1].Split(','))
                {
                    in2.batsman[n] = 0;
                }
            }
            catch(Exception er)
            {
                MessageBox.Show("Please select a valid team.", "Team Selection", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                Console.WriteLine(er.Message);
            }

        }

        private async void playCricket_Click(object sender, RoutedEventArgs e)
        {
            startGrid.Visibility = Visibility.Hidden;
            playGrid.Visibility = Visibility.Visible;

            //Over image Change
            ImageBrush bgimgbr = new ImageBrush();
            Image bgimg = new Image();
            bgimg.Source = new BitmapImage(new Uri("e:/bigData/Images/0.jpg"));
            bgimgbr.ImageSource = bgimg.Source;
            bgimgbr.Opacity = 85;
            playGrid.Background = bgimgbr;
            int g = 0;
            cts = new CancellationTokenSource();
            d1 = distCombo1.SelectedIndex;
            await playMatchInit();
            //VisualStateManager.GoToState(bdp, "zero", true);
            d2 = distCombo2.SelectedIndex;
            t1L.Text = in1.name;
            t2L.Text = in2.name;
            in1.score = 0;
            in2.score = 0;
            await playGameInning1();
            await playGameInning2();
            //Task.WaitAll(playGameInning1(), playGameInning2());
            //MessageBox.Show(inn1.Count.ToString());
            //MessageBox.Show(inn2.Count.ToString());
            //MessageBox.Show(in1.score.ToString());  
            //MessageBox.Show(in2.score.ToString());
            //MessageBox.Show("do");
            int sc = 0;
            double ov = 0.0;
            int ct = 6;
            for (int i = 0; i < inn1.Count; i++)
            {
                if (skip2) { break; }
                else
                {
                    if (ct == 0)
                    {
                        ct = 6;
                        ov = Math.Round(ov);
                        bgimg.Source = new BitmapImage(
                            new Uri("e:/bigData/Images/"+(i%5).ToString()+".jpg"));
                        bgimgbr.ImageSource = bgimg.Source;
                        playGrid.Background = bgimgbr;
                    }
                    batN.Text = inn1[i].Split(';')[1];
                    bowlN.Text = inn1[i].Split(';')[0];
                    ballC.Text = ov.ToString();
                    sc += Int32.Parse(inn1[i].Split(';')[2]);
                    t1R.Text = sc.ToString();
                    ct--;
                    ov += 0.1;
                    VisualStateManager.GoToState(bdp, inn1[i].Split(';')[3], true);
                    await pause(2);
                    VisualStateManager.GoToState(bdp, "none", false);
                    await pause(1);
                }
            }
            sc = 0;
            ov = 0.0;
            ct = 6;
            t1R.Text = in1.score.ToString();
            innL.Text = "Innings 2";
            for (int i = 0; i < inn2.Count; i++)
            {
                if (skip) { MessageBox.Show("wierd"); break; }
                else
                {
                    if (ct == 0)
                    {
                        ct = 6;
                        ov = Math.Round(ov);
                        bgimg.Source = new BitmapImage(
                            new Uri("e:/bigData/Images/" + (i % 5).ToString() + ".jpg"));
                        bgimgbr.ImageSource = bgimg.Source;
                        playGrid.Background = bgimgbr;
                    }
                    batN.Text = inn2[i].Split(';')[1];
                    bowlN.Text = inn2[i].Split(';')[0];
                    ballC.Text = ov.ToString();
                    sc += Int32.Parse(inn2[i].Split(';')[2]);
                    t2R.Text = sc.ToString();
                    ct--;
                    ov += 0.1;
                    VisualStateManager.GoToState(bdp, inn2[i].Split(';')[3], true);
                    await pause(2);
                    VisualStateManager.GoToState(bdp, "none", false);
                    await pause(1);
                }
            }



        }
        private Task pause(double d)
        {
            return Task.Run(() => { Thread.Sleep((int)(d*1000)); });
        }
        private Task playGameInning1()
        {
            return Task.Run(() =>
            {
                bool allOut = false;
                string bt, b1, b2, bw;
                bt = b1 = b2 = bw = "";
                string[] bmen = Team1[1].Split(':')[1].Split(',');
                string[] bwlr = Team2[2].Split(':')[1].Split(',');
                int k = 2;
                b1 = bmen[0];
                b2 = bmen[1];
                bt = b1;
                string op = "", r = "";
                int x = 0;
                for (int i = 0; i < 20; i++)
                {
                    if (allOut) { break; }
                    else
                    {
                        bw = bwlr[i];
                        for (int j = 0; j < 6; j++)
                        {

                            if (!allOut)
                            {
                                op = "";
                                op += bw + ";" + b1 + ";";
                                int cn = clusters[b1];
                                int cnb = clusters[bw];
                                string temp = getR(clusterPvP[cn][cnb].ToArray(), r1[x++]);
                                op += temp;
                                //f.WriteLine(op);
                                inn1.Add(op);
                                in1.score += Int32.Parse(temp.Split(';')[0]);
                                in1.batsman[b1] += Int32.Parse(temp.Split(';')[0]);
                                in1.bowlers[bw][0] += 1;
                                in1.bowlers[bw][1] += Int32.Parse(temp.Split(';')[0]);
                                if (temp.Split(';')[1] == "one" || temp.Split(';')[1] == "three")
                                {
                                    string tp;
                                    tp = b1;
                                    b1 = b2;
                                    b2 = tp;
                                }
                                if (temp.Split(';')[1] == "wick")
                                {
                                    in1.bowlers[bw][2] += 1;
                                    if (k == 11)
                                    {
                                        allOut = true;
                                    }
                                    else
                                    {
                                        b1 = bmen[k];
                                        k++;
                                    }
                                }
                            }
                            else { break; }

                            //for()
                        }
                        string tp1;
                        tp1 = b1;
                        b1 = b2;
                        b2 = tp1;
                    }
                }
            });
        }
        private Task playGameInning2()
        {
            return Task.Run(() =>
            {
                bool allOut = false;
                string bt, b1, b2, bw;
                bt = b1 = b2 = bw = "";
                string[] bmen = Team2[1].Split(':')[1].Split(',');
                string[] bwlr = Team1[2].Split(':')[1].Split(',');
                int k = 2;
                b1 = bmen[0];
                b2 = bmen[1];
                bt = b1;
                string op = "", r = "";
                int x = 0;
                for (int i = 0; i < 20; i++)
                {
                    if (allOut) { break; }
                    else
                    {
                        bw = bwlr[i];
                        for (int j = 0; j < 6; j++)
                        {

                            if (!allOut)
                            {
                                op = "";
                                op += bw + ";" + b1 + ";";
                                int cn = clusters[b1];
                                int cnb = clusters[bw];
                                string temp = getR(clusterPvP[cn][cnb].ToArray(), r2[x++]);
                                op += temp;
                                //f.WriteLine(op);
                                inn2.Add(op);
                                in2.score += Int32.Parse(temp.Split(';')[0]);
                                if (in2.score > in1.score) { allOut = true; }
                                else
                                {
                                    in2.batsman[b1] += Int32.Parse(temp.Split(';')[0]);
                                    in2.bowlers[bw][0] += 1;
                                    in2.bowlers[bw][1] += Int32.Parse(temp.Split(';')[0]);
                                    if (temp.Split(';')[1] == "one" || temp.Split(';')[1] == "three")
                                    {
                                        string tp;
                                        tp = b1;
                                        b1 = b2;
                                        b2 = tp;
                                    }
                                    if (temp.Split(';')[1] == "wick")
                                    {
                                        in2.bowlers[bw][2] += 1;
                                        if (k == 11)
                                        {
                                            allOut = true;
                                        }
                                        else
                                        {
                                            b1 = bmen[k];
                                            k++;
                                        }
                                    }
                                }
                            }
                            else { break; }

                            //for()
                        }
                        string tp1;
                        tp1 = b1;
                        b1 = b2;
                        b2 = tp1;
                    }
                }
            });
        }
        private string getR(double[] pvp,double n)
        {
            debug += "rand: " + n.ToString() + Environment.NewLine + "arr: " + string.Join(",", pvp)+Environment.NewLine;
            string op = "";
            if (n <= pvp[0])
            {
                op += "0;zero";
            }
            else if (n <= pvp[1])
            {
                op += "1;one";
            }
            else if (n <= pvp[2])
            {
                op += "2;two";
            }
            else if (n <= pvp[3])
            {
                op += "3;three";
            }
            else if (n <= pvp[4])
            {
                op += "4;four";
            }
            else if (n <= pvp[5])
            {
                op += "6;six";
            }
            else
            {
                op += "0;wick";
            }
            return op;
        }

        private void skIn1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("jjj");
            skip = true;
        }

        private void skIn2_Click(object sender, RoutedEventArgs e)
        {
            skip2 = true;
        }

        private void res_Click(object sender, RoutedEventArgs e)
        {
            playGrid.Visibility = Visibility.Hidden;
            ResultsGrid.Visibility = Visibility.Visible;
            int w1, w2 = 0;
            w1 = 0;
            foreach(var key in in1.bowlers.Keys)
            {
                w1 += in1.bowlers[key][2];
            }
            foreach (var key in in2.bowlers.Keys)
            {
                w2 += in2.bowlers[key][2];
            }
            t1N.Text = in1.name + " : " + in1.score + "/" + w1.ToString();
            t2N.Text = in2.name + " : " + in2.score + "/" + w2.ToString();
            string b1 = "Bowlers: Ball Runs Wickets"+Environment.NewLine, b2 = "Bowlers: Ball Runs Wickets"+Environment.NewLine;
            foreach(var key in in1.bowlers.Keys)
            {
                b1 += key + " " + in1.bowlers[key][0] + " " + in1.bowlers[key][1] + " " + in1.bowlers[key][2]+Environment.NewLine;
            }
            foreach (var key in in2.bowlers.Keys)
            {
                b2 += key + " " + in2.bowlers[key][0] + " " + in2.bowlers[key][1] + " " + in2.bowlers[key][2] + Environment.NewLine;
            }
            t2Bowl.Text = b1;
            t1Bowl.Text = b2;
            b1 =b2= "Batsman:" + Environment.NewLine;
            foreach(var key in in1.batsman.Keys)
            {
                if (in1.batsman[key] != 0)
                {
                    b1 += key + " | " + in1.batsman[key] + " Runs" + Environment.NewLine;
                }
            }
            foreach (var key in in2.batsman.Keys)
            {
                if (in2.batsman[key] != 0)
                {
                    b2 += key + " | " + in2.batsman[key] + " Runs" + Environment.NewLine;
                }
            }
            t1Bat.Text = b1;
            t2Bat.Text = b2;
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private Task playMatchInit()
        {
            
            return Task.Run(() =>
            {
                Random rand1 = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                Random rand2 = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                ContinuousUniform.Samples(rand1,r1, 0.0, 1.0);
                if (d1 == 0)
                {
                    //ContinuousUniform.Samples(r1, 0.0, 0.1);
                }
                else if (d1 == 1)
                {
                    r1 = r1.Select(z => Math.Sqrt(z)).ToArray();
                }
                else if (d1 == 2)
                {
                    r1 = r1.Select(z => z*z).ToArray();
                }
                ContinuousUniform.Samples(rand2,r2, 0.0, 1.0);
                if (d2 == 0)
                {
                    //ContinuousUniform.Samples(r1, 0.0, 0.1);
                }
                else if (d2 == 1)
                {
                    r2 = r2.Select(z => Math.Sqrt(z)).ToArray();
                }
                else if (d2 == 2)
                {
                    r2 = r2.Select(z => z * z).ToArray();
                }
                foreach (string n in Team1[2].Split(':')[1].Split(','))
                {
                    in2.bowlers[n] = new int[3];
                }
                foreach (string n in in2.bowlers.Keys)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        in2.bowlers[n][i] = 0;
                    }
                }
                foreach (string n in Team2[2].Split(':')[1].Split(','))
                {
                    in1.bowlers[n] = new int[3];
                }
                foreach (string n in in1.bowlers.Keys)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        in1.bowlers[n][i] = 0;
                    }
                }
            });
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                initStart();
            }
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private void team1select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //replace text with default directory where you're keeping teams
            try
            {
                openFileDialog1.InitialDirectory = "e:\\bigData\\";

                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

                openFileDialog1.FilterIndex = 2;
                openFileDialog1.ShowDialog();
                Team1 = File.ReadAllLines(openFileDialog1.FileName).ToList();
                t1selConf.Visibility = Visibility.Visible;
                in1.name = openFileDialog1.SafeFileName.Replace(".txt", "");
                in1.batsman = new Dictionary<string, int>();
                in1.bowlers = new Dictionary<string, int[]>();

                foreach (string n in Team1[1].Split(':')[1].Split(','))
                {
                    in1.batsman[n] = 0;
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("Please select a valid team.", "Team Selection", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                Console.WriteLine(er.Message);
            }
            

        }
    }
}
