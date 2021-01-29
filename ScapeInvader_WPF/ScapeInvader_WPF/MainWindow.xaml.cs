using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;
namespace ScapeInvader_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool GoLeft, GoRight;//向左，向右
        private List<Rectangle> itemsToRemove = new List<Rectangle>();
        int enemyImage = 0;
        int bulletTime = 0;
        int bulletTimerLimit = 90;//子弹间隔时间
        int totalEnemies = 0;
        int enemySpeed = 6;
        bool GameOver = false;

        DispatcherTimer GameTimer = new DispatcherTimer();//游戏时长计时器
        ImageBrush PlayerSkin = new ImageBrush();//玩家皮肤

        public MainWindow()
        {
            InitializeComponent();
            GameTimer.Tick += GameLoop;
            GameTimer.Interval = TimeSpan.FromMilliseconds(20);
            GameTimer.Start();
            PlayerSkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/player.png"));
            Player.Fill=PlayerSkin;

            myCanvas.Focus();//使画布集中

            MakeEnemies(30);
        }
        /// <summary>
        /// 游戏界面的显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop(object sender, EventArgs e)
        {
            #region 玩家移动
            Rect PlayerHitBox = new Rect(Canvas.GetLeft(Player),Canvas.GetTop(Player),Player.Width,Player.Height);
            EnemiesLeft.Content = "剩余敌人：" + totalEnemies;
            if (GoLeft==true && Canvas.GetLeft(Player)>0)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - 9);
            }
            if(GoRight==true && Canvas.GetLeft(Player) + 80 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + 9);
            }
            #endregion

            #region //敌人子弹产生
            bulletTime -= 3;
            if(bulletTime<0)
            {
                EnemyBulletMaker(Canvas.GetLeft(Player)+10,20);
                bulletTime = bulletTimerLimit;
            }
            #endregion

            foreach ( var x in myCanvas.Children.OfType<Rectangle>())
            {
                //子弹向上的移动
                if(x is Rectangle && (string)x.Tag=="bullet")
                {
                    Canvas.SetTop(x,Canvas.GetTop(x)-20);
                    if(Canvas.GetTop(x) < 10)
                    {
                        itemsToRemove.Add(x);
                    }
                    Rect bulletBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    foreach(var y in myCanvas.Children.OfType<Rectangle>())
                    {
                        if(y is Rectangle && (string)y.Tag=="Enemy")
                        {
                            Rect EnemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);
                            if(bulletBox.IntersectsWith(EnemyHit))
                            {
                                itemsToRemove.Add(x);
                                itemsToRemove.Add(y);
                                totalEnemies -= 1;
                            }
                        }
                    }
                }
                //敌人产生移动
                if(x is Rectangle && (string)x.Tag=="Enemy")
                {
                    Canvas.SetLeft(x,Canvas.GetLeft(x)+enemySpeed);
                    if(Canvas.GetLeft(x)>820)
                    {
                        Canvas.SetLeft(x,-80);
                        Canvas.SetTop(x, Canvas.GetTop(x) + (x.Height+10));//循环出来让他往下移10个像素点
                    }
                    Rect enemyHitBox = new Rect(Canvas.GetLeft(x),Canvas.GetTop(x),x.Width,x.Height);

                    if(PlayerHitBox.IntersectsWith(enemyHitBox))//检测是否碰撞
                    {
                        ShowGameOver("敌人击杀了你！！！");
                    }
                }

                //敌人子弹
                if(x is Rectangle && (string)x.Tag == "EnemyBullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 10);
                    if(Canvas.GetTop(x) > 480)
                    {
                        itemsToRemove.Add(x);
                    }
                    Rect enemyBulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (PlayerHitBox.IntersectsWith(enemyBulletHitBox))//检测是否碰撞
                    {
                        ShowGameOver("敌人击杀了你！！！");
                    }
                }
            }
                
            foreach(Rectangle i in itemsToRemove)
            {
                myCanvas.Children.Remove(i);
            }

            if(totalEnemies<10)
            {
                enemySpeed =10;
            }
            if(totalEnemies<1)
            {
                ShowGameOver("我们还会回来的，呀哇哇哇！！！");
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                GoLeft = true;
            }
            if(e.Key==Key.Right)
            {
                GoRight = true;
            }
            //空格键开火---玩家子弹
            if(e.Key==Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Width = 5,
                    Height=20,
                    Fill=Brushes.Purple,//子弹主体颜色
                   // Stroke=Brushes.Red,//外部描边颜色
                };
                Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + Player.Width / 2);
                Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);
                myCanvas.Children.Add(newBullet);
            }
            if(e.Key==Key.Enter && GameOver==true)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                GoLeft = false;
            }
            if (e.Key == Key.Right)
            {
                GoRight = false;
            }
        }
        /// <summary>
        /// 敌人子弹的产生
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void EnemyBulletMaker(double x,double y)
        {
            Rectangle enemyBullet = new Rectangle
            {
                Tag = "EnemyBullet",
                Width=6,
                Height=21,
                Fill=Brushes.Purple,
                Stroke=Brushes.Yellow,
            };
            myCanvas.Children.Add(enemyBullet);
            Canvas.SetLeft(enemyBullet,x);
            Canvas.SetTop(enemyBullet,y);
        }
        /// <summary>
        /// 制造敌人
        /// </summary>
        /// <param name="limit">限制敌人的数量</param>
        private void MakeEnemies(int limit)
        {
            int left = 0;//
            totalEnemies = limit;
            for (int i = 0; i < limit; i++)
            {
                ImageBrush enemySkin = new ImageBrush();
                Rectangle newEnemy = new Rectangle
                {
                    Tag = "Enemy",
                    Width = 40,
                    Height = 40,
                    Fill = enemySkin,
                };
                left -= 60;//长度减60
                Canvas.SetTop(newEnemy, 10);//距离顶部的位置
                Canvas.SetLeft(newEnemy, left);
                myCanvas.Children.Add(newEnemy);

                enemyImage++;

                if(enemyImage>5)
                {
                    enemyImage = 1;
                }
                //敌人图片的选择
                switch(enemyImage)
                {
                    case 1:
                        enemySkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/1.png")); //pack://application:,,,/images/player.png"
                        break;
                    case 2:
                        enemySkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/2.png"));
                        break;
                    case 3:
                        enemySkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/3.png"));
                        break;
                    case 4:
                        enemySkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/4.png"));
                        break;
                    case 5:
                        enemySkin.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/5.png"));
                        break;
                }
            }
            
        }

        private void ShowGameOver(string value)
        {
            GameOver = true;
            GameTimer.Stop();
            EnemiesLeft.Content += "" + value + "按下enter键重来！";
        }
    }
}
