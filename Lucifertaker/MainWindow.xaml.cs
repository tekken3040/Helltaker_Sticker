using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lucifertaker
{
    public partial class MainWindow : Window
    {
        Dictionary<int, ImageSource[]> BitmapResouce = new Dictionary<int, ImageSource[]>();    // 비트맵 스프라이트 저장 딕셔너리
        Bitmap[] iCons = new Bitmap[Header.CHARACTER_NUM];                                      // 트레이 아이콘
        int frame = -1;                                                                         // 애니메이션 프레임
        int currentImg = (int)Header.CharacterSprite.Lucifer;                                   // 현재 재생중인 이미지
        DispatcherTimer timer;                                                                  // 애니 재생을 위한 타이머
        StringBuilder strBuilder = new StringBuilder();                                         // 트레이 메뉴 이름을 위한 스트링 빌더
        SoundPlayer soundPlayer = new SoundPlayer();                                            // 사운드 재생을 위한 플레이어

        /* for release bitmap */
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        
        public static extern bool DeleteObject([In] IntPtr hObject);

        public MainWindow()
        {
            InitializeComponent();
            BitmapInit();
            NotifyTraySetup();

            // 윈도우를 항상 위로
            this.Topmost = true;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.0167 * 3); // 60fps
            timer.Tick += NextFrame;
            timer.Start();

            MouseDown += MainWindow_MouseDown;
        }

        System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();     // 트레이 메뉴
        System.Windows.Forms.NotifyIcon noti;                                               // 트레이 아이콘
        // 트레이 셋업
        private void NotifyTraySetup()
        {
            noti = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.FromHandle(iCons[currentImg].GetHicon()),
                Visible = true,
                Text = "둠칫 둠칫",
                ContextMenu = menu,
            };

            TrayMenuSetup();

            noti.ContextMenu = menu;
        }

        string[] characterName = new string[Header.CHARACTER_NUM +1];                       // 케르베로스(자매) 때문에 1 추가
        string[] musicName = new string[Header.MUSIC_NUM];                                  // 사운드 이름
        System.IO.Stream[] musicStreams = new System.IO.Stream[Header.MUSIC_NUM];           // 사운드를 저장할 스트림
        // 트레이 메뉴 설정
        private void TrayMenuSetup()
        {
            ImageTrayInit();
            SoundTrayInit();

            // 종료를 위한 트레이 메뉴
            var item = new System.Windows.Forms.MenuItem
            {
                Index = 2,
                Text = "잘가 루시퍼쨔응",
            };
            item.Click += (object o, EventArgs e) => { System.Windows.Application.Current.Shutdown(); };
            menu.MenuItems.Add(item);
        }

        // 사운드 리스트 설정
        private void SoundTrayInit()
        {
            // 트레이 사운드 이름
            musicName[0] = "Vitality";
            musicName[1] = "Apropos";
            musicName[2] = "Epitomize";
            musicName[3] = "Luminescent";

            // 사운드 스트림
            musicStreams[0] = Properties.Resources.Mittsies___Helltaker_Soundtrack___01_Vitality;
            musicStreams[1] = Properties.Resources.Mittsies___Helltaker_Soundtrack___02_Apropos;
            musicStreams[2] = Properties.Resources.Mittsies___Helltaker_Soundtrack___03_Epitomize;
            musicStreams[3] = Properties.Resources.Mittsies___Helltaker_Soundtrack___04_Luminescent;

            // 사운드 재생 초기화
            System.Windows.Forms.MenuItem[] music = new System.Windows.Forms.MenuItem[Header.MUSIC_NUM + 1];
            for (int i = 0; i < music.Length - 1; i++)
            {
                music[i] = new System.Windows.Forms.MenuItem(musicName[i]) { Index = i };
                music[i].Click += (object o, EventArgs e) =>
                {
                    for (int j = 0; j < music.Length - 1; j++)
                        music[j].Checked = false;
                    ((System.Windows.Forms.MenuItem)o).Checked = true;
                    musicStreams[((System.Windows.Forms.MenuItem)o).Index].Position = 0;
                    soundPlayer.Stream = musicStreams[((System.Windows.Forms.MenuItem)o).Index];
                    timer.Stop();
                    soundPlayer.PlayLooping();
                    timer.Start();
                };
            }

            // 재생중지
            music[4] = new System.Windows.Forms.MenuItem("노래 그만듣기") { Index = 4 };
            music[4].Click += (object o, EventArgs e) =>
            {
                for (int i = 0; i < music.Length - 1; i++)
                    music[i].Checked = false;
                soundPlayer.Stop();
            };

            var item = new System.Windows.Forms.MenuItem("노래 선택♡", music);
            item.Index = 1;
            menu.MenuItems.Add(item);
        }

        // 캐릭터 리스트 설정
        private void ImageTrayInit()
        {
            // 트레이 캐릭터 메뉴 이름
            characterName[0] = "아자젤";
            characterName[1] = "케르베로스";
            characterName[2] = "케르베로스(자매)";
            characterName[3] = "저지먼트";
            characterName[4] = "저스티스";
            characterName[5] = "루시퍼";
            characterName[6] = "루시퍼(앞치마)";
            characterName[7] = "말리나";
            characterName[8] = "모데우스";
            characterName[9] = "판데모니카";
            characterName[10] = "즈드라다";

            // 캐릭터 선택 초기화
            // 케르베로스(자매) 때문에 Header.CHARACTER_NUM에1추가
            System.Windows.Forms.MenuItem[] character = new System.Windows.Forms.MenuItem[Header.CHARACTER_NUM + 1];
            for (int i = 0; i < character.Length; i++)
            {
                character[i] = new System.Windows.Forms.MenuItem(characterName[i]) { Index = i };
                character[i].Click += (object o, EventArgs e) => {
                    currentImg = ((System.Windows.Forms.MenuItem)o).Index;
                    for (int j = 0; j < character.Length; j++)
                        character[j].Checked = false;
                    ((System.Windows.Forms.MenuItem)o).Checked = true;
                    strBuilder.Clear();
                    strBuilder.Append("잘가 ").Append(characterName[currentImg]).Append("쨔응");
                    menu.MenuItems[2].Text = strBuilder.ToString();
                    if (currentImg == (int)Header.CharacterSprite.Cerberus3)
                    {
                        currentImg = (int)Header.CharacterSprite.Cerberus;
                        iLucifer2.Visibility = Visibility.Visible;
                        iLucifer3.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        iLucifer2.Visibility = Visibility.Hidden;
                        iLucifer3.Visibility = Visibility.Hidden;
                    }

                    noti.Icon = System.Drawing.Icon.FromHandle(iCons[currentImg].GetHicon());
                };
            }

            character[(int)Header.CharacterSprite.Lucifer].Checked = true;

            // 최상단 메뉴
            var item = new System.Windows.Forms.MenuItem("악마 선택♡", character);
            item.Index = 0;
            menu.MenuItems.Add(item);
        }

        // 캐릭터 생성을 위한 이미지 할당
        private void BitmapInit()
        {
            // 불러온 원본 스프라이트
            Bitmap original;

            // 케르베로스를 제외한 나머지는 한명만 등장함으로 두 슬롯은 감추기
            iLucifer2.Visibility = Visibility.Hidden;
            iLucifer3.Visibility = Visibility.Hidden;

            // 비트맵 아틀라스 초기화
            for (int i=0; i< Header.CHARACTER_NUM; i++)
            {
                original = GetCharacterSprite(i);
                Bitmap[] frames = new Bitmap[Header.ANI_FRAME];
                ImageSource[] imgFrame = new ImageSource[Header.ANI_FRAME];

                for (int j = 0; j < Header.ANI_FRAME; j++)
                {
                    frames[j] = new Bitmap(100, 100);
                    using (Graphics g = Graphics.FromImage(frames[j]))
                    {
                        // 루시퍼 앞치마 버전은 루시퍼 스프라이트의 두번재 줄에 위치함으로 따로 잘라준다
                        if(i == (int)Header.CharacterSprite.LuciferApron)
                        {
                            g.DrawImage(original,
                            new System.Drawing.Rectangle(0, 0, 100, 100),
                            new System.Drawing.Rectangle(j * 100, 100, 100, 100),
                            GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.DrawImage(original,
                            new System.Drawing.Rectangle(0, 0, 100, 100),
                            new System.Drawing.Rectangle(j * 100, 0, 100, 100),
                            GraphicsUnit.Pixel);
                        }
                    }
                    
                    var handle = frames[j].GetHbitmap();
                    try
                    {
                        imgFrame[j] = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        DeleteObject(handle);
                    }
                }
                // 윈도우 트레이에 보여질 아이콘은 각 캐릭터의 첫번째 프레임
                iCons[i] = frames[0];

                BitmapResouce.Add(i, imgFrame);
            }
        }

        // 캐릭터 스프라이트 얻기
        private Bitmap GetCharacterSprite(int spriteNum)
        {
            Bitmap bitmap = Properties.Resources.Azazel;

            switch(spriteNum)
            {
                case (int)Header.CharacterSprite.Azazel:
                    bitmap = Properties.Resources.Azazel;
                    break;
                case (int)Header.CharacterSprite.Cerberus:
                    bitmap = Properties.Resources.Cerberus;
                    break;
                case (int)Header.CharacterSprite.Judgement:
                    bitmap = Properties.Resources.Judgement;
                    break;
                case (int)Header.CharacterSprite.Justice:
                    bitmap = Properties.Resources.Justice;
                    break;
                case (int)Header.CharacterSprite.Lucifer:
                    bitmap = Properties.Resources.Lucifer;
                    break;
                case (int)Header.CharacterSprite.LuciferApron:
                    bitmap = Properties.Resources.Lucifer;
                    break;
                case (int)Header.CharacterSprite.Malina:
                    bitmap = Properties.Resources.Malina;
                    break;
                case (int)Header.CharacterSprite.Modeus:
                    bitmap = Properties.Resources.Modeus;
                    break;
                case (int)Header.CharacterSprite.Pandemonica:
                    bitmap = Properties.Resources.Pandemonica;
                    break;
                case (int)Header.CharacterSprite.Zdrada:
                    bitmap = Properties.Resources.Zdrada;
                    break;
            }

            return bitmap;
        }

        // 마우스 드래그
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // 다음 프레임으로 애니메이션
        private void NextFrame(object sender, EventArgs e)
        {
            frame = (frame + 1) % Header.ANI_FRAME;
            iLucifer.Source = BitmapResouce[currentImg][frame];
            iLucifer2.Source = BitmapResouce[currentImg][frame];
            iLucifer3.Source = BitmapResouce[currentImg][frame];
        }
    }
}
