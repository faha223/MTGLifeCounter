using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using MTGLifeCounter.AnimationHelpers;
using Android.Views.Animations;
using System.Timers;
using Android.Content.PM;
using System.Collections.Generic;
using System.Linq;

namespace MTGLifeCounter
{
    [Activity(Label = "MTGLifeCounter", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        #region Settings

        private static readonly int defaultBaseHealth = 20;

        private int baseHealth = defaultBaseHealth;
        public int BaseHealth
        {
            get
            {
                return baseHealth;
            }
            set
            {
                if (baseHealth != value)
                {
                    baseHealth = value;
                    OnPropertyChanged("BaseHealth");
                }
            }
        }

        private int playerBackground;
        public int PlayerBackground
        {
            get
            {
                return playerBackground;
            }
            set
            {
                if (playerBackground != value)
                {
                    playerBackground = value;
                    OnPropertyChanged("PlayerBackground");
                    OnPropertyChanged("PlayerDarkMode");
                }
            }
        }

        private int opponentBackground;
        public int OpponentBackground
        {
            get
            {
                return opponentBackground;
            }
            set
            {
                if (opponentBackground != value)
                {
                    opponentBackground = value;
                    OnPropertyChanged("OpponentBackground");
                    OnPropertyChanged("OpponentDarkMode");
                }
            }
        }

        public bool PlayerDarkMode
        {
            get
            {
                return (playerBackground == Resource.Drawable.plainsBg);
            }    
        }

        public bool OpponentDarkMode
        {
            get
            {
                return (opponentBackground == Resource.Drawable.plainsBg);
            }
        }

        #endregion Settings

        #region Main View

        private int opponentHealth = defaultBaseHealth;
        public int OpponentHealth
        {
            get
            {
                return opponentHealth;
            }
            set
            {
                if (opponentHealth != value)
                {
                    opponentHealth = value;
                    OnPropertyChanged("OpponentHealth");
                }
            }
        }

        private int playerHealth = defaultBaseHealth;
        public int PlayerHealth
        {
            get
            {
                return playerHealth;
            }
            set
            {
                if (playerHealth != value)
                {
                    playerHealth = value;
                    OnPropertyChanged("PlayerHealth");
                }
            }
        }

        #endregion Main View

        #region Dice View

        private Random Rand = new Random();
        private int DiceRollCount = 13;
        private object diceTimerLock = new object();
        private Timer diceTimer;

        private bool showDice;
        public bool ShowDice
        {
            get
            {
                return showDice;
            }
            set
            {
                if (showDice != value)
                {
                    showDice = value;
                    OnPropertyChanged("ShowDice");
                }
            }
        }

        private int playerDie = 1;
        public int PlayerDie
        {
            get
            {
                return playerDie;
            }
            set
            {
                if (playerDie != value)
                {
                    playerDie = value;
                    OnPropertyChanged("PlayerDie");
                }
            }
        }

        private int opponentDie = 1;
        public int OpponentDie
        {
            get
            {
                return opponentDie;
            }
            set
            {
                if (opponentDie != value)
                {
                    opponentDie = value;
                    OnPropertyChanged("OpponentDie");
                }
            }
        }

        private int diceRollWinner;
        public int DiceRollWinner
        {
            get
            {
                return diceRollWinner;
            }
            set
            {
                if (diceRollWinner != value)
                {
                    diceRollWinner = value;
                    OnPropertyChanged("DiceRollWinner");
                }
            }
        }

        #endregion Dice View

        #region Select Background View

        private bool _showPlayerSelectBackgroundView;
        public bool ShowPlayerSelectBackgroundView
        {
            get
            {
                return _showPlayerSelectBackgroundView;
            }
            set
            {
                if (_showPlayerSelectBackgroundView != value)
                {
                    _showPlayerSelectBackgroundView = value;
                    OnPropertyChanged("ShowPlayerSelectBackgroundView");
                }
            }
        }

        private bool _showOpponentSelectBackgroundView;
        public bool ShowOpponentSelectBackgroundView
        {
            get
            {
                return _showOpponentSelectBackgroundView;
            }
            set
            {
                if (_showOpponentSelectBackgroundView != value)
                {
                    _showOpponentSelectBackgroundView = value;
                    OnPropertyChanged("ShowOpponentSelectBackgroundView");
                }
            }
        }

        #endregion Select Background View

        private bool showMenu;
        public bool ShowMenu
        {
            get
            {
                return showMenu;
            }
            set
            {
                if (showMenu != value)
                {
                    showMenu = value;
                    OnPropertyChanged("ShowMenu");
                }
            }
        }

        private bool showLifeMenu;
        public bool ShowLifeMenu
        {
            get
            {
                return showLifeMenu;
            }
            set
            {
                if (showLifeMenu != value)
                {
                    showLifeMenu = value;
                    OnPropertyChanged("ShowLifeMenu");
                }
            }
        }

        private Animation ResetHealthAnimation
        {
            get
            {
                var anim = new ShakeAnimation(64.0f, 9);
                anim.Duration = 300;
                return anim;
            }
        }

        private Animation DieChangeAnimation
        {
            get
            {
                var anim = new ShakeAnimation(16.0f, 1);
                anim.Duration = 125;
                return anim;
            }
        }

        private Animation OpenMenuAnimation
        {
            get
            {
                var anim = new ScaleAnimation(1.0f, 1.0f, 0.0f, 1.0f);
                anim.Duration = 250;
                return anim;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.KeepScreenOn, WindowManagerFlags.Fullscreen | WindowManagerFlags.KeepScreenOn);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Hook up the commands to the buttons
            FindViewById<ImageButton>(Resource.Id.mv_OpponentIncreaseHealth).Click += IncreaseOpponentHealthCommand;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentDecreaseHealth).Click += DecreaseOpponentHealthCommand;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerIncreaseHealth).Click += IncreasePlayerHealthCommand;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerDecreaseHealth).Click += DecreasePlayerHealthCommand;
            FindViewById<Button>(Resource.Id.mv_PlayerToggleSelectBg).Click += TogglePlayerSelectBgCommand;
            FindViewById<Button>(Resource.Id.mv_OpponentToggleSelectBg).Click += ToggleOpponentSelectBgCommand;

            // Menu Open/Close
            FindViewById<ImageButton>(Resource.Id.mv_menuToggle).Click += ToggleMenuCommand;
            FindViewById<Button>(Resource.Id.mv_menuClose).Click += CloseMenuCommand;
            
            // Menu Buttons
            FindViewById<Button>(Resource.Id.mv_menu_reset).Click += ResetCommand;
            FindViewById<Button>(Resource.Id.mv_menu_dice).Click += DiceCommand;
            FindViewById<Button>(Resource.Id.mv_menu_life).Click += LifeCommand;
            FindViewById<Button>(Resource.Id.mv_menu_settings).Click += SettingsCommand;

            // Life Menu Buttons
            FindViewById<Button>(Resource.Id.mv_setLife20).Click += SetLife20Command;
            FindViewById<Button>(Resource.Id.mv_setLife30).Click += SetLife30Command;
            FindViewById<Button>(Resource.Id.mv_setLife40).Click += SetLife40Command;

            // SelectBgButtons
            FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgFst).Click += SetPlayerBgForest;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgPln).Click += SetPlayerBgPlains;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgMtn).Click += SetPlayerBgMountain;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgSmp).Click += SetPlayerBgSwamp;
            FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgWtr).Click += SetPlayerBgWater;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgFst).Click += SetOpponentBgForest;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgPln).Click += SetOpponentBgPlains;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgMtn).Click += SetOpponentBgMountain;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgSmp).Click += SetOpponentBgSwamp;
            FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgWtr).Click += SetOpponentBgWater;

            // Hide nav buttons
            FindViewById(Resource.Id.mv_LayoutRoot).SystemUiVisibility = StatusBarVisibility.Hidden;

            PlayerBackground = Resource.Drawable.plainsBg;
            OpponentBackground = Resource.Drawable.swampBg;
        }

        #region Main View Commands

        private void IncreaseOpponentHealthCommand(object sender, EventArgs args)
        {
            OpponentHealth++;
        }
        private void DecreaseOpponentHealthCommand(object sender, EventArgs args)
        {
            OpponentHealth--;
        }
        private void IncreasePlayerHealthCommand(object sender, EventArgs args)
        {
            PlayerHealth++;
        }
        private void DecreasePlayerHealthCommand(object sender, EventArgs args)
        {
            PlayerHealth--;
        }

        #endregion Main View Commands

        #region Menu Commands

        private void ToggleMenuCommand(object sender, EventArgs args)
        {
            ShowMenu = !ShowMenu;
        }

        private void CloseMenuCommand(object sender, EventArgs args)
        {
            ShowMenu = false;
            ShowLifeMenu = false;
        }

        private void ResetCommand(object sender, EventArgs args)
        {
            PlayerHealth = BaseHealth;
            OpponentHealth = BaseHealth;
            RunOnUiThread(() =>
            {
                FindViewById(Resource.Id.mv_PlayerHealth).StartAnimation(ResetHealthAnimation);
                FindViewById(Resource.Id.mv_OpponentHealth).StartAnimation(ResetHealthAnimation);
            });
        }

        private void DiceCommand(object sender, EventArgs args)
        {
            DiceRollCount = 13;
            DiceRollWinner = 0;
            ShowDice = true;

            lock (diceTimerLock)
            {
                if (diceTimer != null)
                {
                    diceTimer.Stop();
                    diceTimer.Dispose();
                    diceTimer = null;
                }
                diceTimer = new Timer();
                diceTimer.Interval = 125;
                diceTimer.Elapsed += delegate
                {
                    lock (diceTimerLock)
                    {
                        diceTimer.Interval *= 1.15;
                        DiceRollCount--;
                        RollDice();

                        // No Ties
                        if ((DiceRollCount == 0) && (PlayerDie == OpponentDie))
                        {
                            do
                            {
                                RollDice();
                            } while (PlayerDie == OpponentDie);
                        }

                        if (DiceRollCount == 0)
                        {
                            DiceRollWinner = (PlayerDie > OpponentDie) ? 1 : 2;
                            DiceRollCount = 13;
                            diceTimer.Stop();
                            diceTimer.Dispose();

                            diceTimer = new Timer();
                            diceTimer.Interval = 1500;
                            diceTimer.Elapsed += delegate
                            {
                                lock (diceTimerLock)
                                {
                                    ShowDice = false;
                                    diceTimer.Stop();
                                    diceTimer.Dispose();
                                    diceTimer = null;
                                }
                            };
                            diceTimer.Start();
                        }
                    }
                };
                diceTimer.Start();
            }
        }

        private void LifeCommand(object sender, EventArgs args)
        {
            ShowLifeMenu = true;
        }

        private void SettingsCommand(object sender, EventArgs args)
        {

        }

        #endregion Menu Commands

        #region Life Menu Commands

        private void SetLife20Command(object sender, EventArgs args)
        {
            BaseHealth = 20;
            ShowLifeMenu = false;
            ResetCommand(sender, args);
        }

        private void SetLife30Command(object sender, EventArgs args)
        {
            BaseHealth = 30;
            ShowLifeMenu = false;
            ResetCommand(sender, args);
        }

        private void SetLife40Command(object sender, EventArgs args)
        {
            BaseHealth = 40;
            ShowLifeMenu = false;
            ResetCommand(sender, args);
        }

        #endregion Life Menu Commands

        #region Select Background Commands

        void TogglePlayerSelectBgCommand(object sender, EventArgs args)
        {
            ShowPlayerSelectBackgroundView = !ShowPlayerSelectBackgroundView;
        }

        void ToggleOpponentSelectBgCommand(object sender, EventArgs args)
        {
            ShowOpponentSelectBackgroundView = !ShowOpponentSelectBackgroundView;
        }

        void SetPlayerBgForest(object sender, EventArgs args)
        {
            PlayerBackground = Resource.Drawable.forestBg;
        }

        void SetPlayerBgMountain(object sender, EventArgs args)
        {
            PlayerBackground = Resource.Drawable.mountainBg;
        }

        void SetPlayerBgWater(object sender, EventArgs args)
        {
            PlayerBackground = Resource.Drawable.waterBg;
        }

        void SetPlayerBgPlains(object sender, EventArgs args)
        {
            PlayerBackground = Resource.Drawable.plainsBg;
        }

        void SetPlayerBgSwamp(object sender, EventArgs args)
        {
            PlayerBackground = Resource.Drawable.swampBg;
        }

        void SetOpponentBgForest(object sender, EventArgs args)
        {
            OpponentBackground = Resource.Drawable.forestBg;
        }

        void SetOpponentBgMountain(object sender, EventArgs args)
        {
            OpponentBackground = Resource.Drawable.mountainBg;
        }

        void SetOpponentBgWater(object sender, EventArgs args)
        {
            OpponentBackground = Resource.Drawable.waterBg;
        }

        void SetOpponentBgPlains(object sender, EventArgs args)
        {
            OpponentBackground = Resource.Drawable.plainsBg;
        }

        void SetOpponentBgSwamp(object sender, EventArgs args)
        {
            OpponentBackground = Resource.Drawable.swampBg;
        }

        #endregion

        private void OnPropertyChanged(string property)
        {
            switch(property)
            {
                case "PlayerHealth":
                    RunOnUiThread(() =>
                    {
                        UpdateHealth(Resource.Id.mv_PlayerHealth, PlayerHealth);
                    });
                    break;
                case "OpponentHealth":
                    RunOnUiThread(() =>
                    {
                        UpdateHealth(Resource.Id.mv_OpponentHealth, OpponentHealth);
                    });
                    break;
                case "ShowMenu":
                    RunOnUiThread(() =>
                    {
                        FindViewById<Button>(Resource.Id.mv_menuClose).Visibility = (ShowMenu ? ViewStates.Visible : ViewStates.Gone);
                        var menu = FindViewById<LinearLayout>(Resource.Id.mv_menu);
                        menu.Visibility = (ShowMenu ? ViewStates.Visible : ViewStates.Gone);
                        if (ShowMenu)
                            menu.StartAnimation(OpenMenuAnimation);
                    });
                    break;
                case "ShowLifeMenu":
                    RunOnUiThread(() =>
                    {
                        if (ShowLifeMenu)
                        {
                            FindViewById(Resource.Id.mv_lifeMenu).Visibility = ViewStates.Visible;
                            FindViewById(Resource.Id.mv_menu).Visibility = ViewStates.Gone;
                        }
                        else if (ShowMenu)
                        {
                            FindViewById(Resource.Id.mv_lifeMenu).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_menu).Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            FindViewById(Resource.Id.mv_lifeMenu).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_menu).Visibility = ViewStates.Gone;
                        }
                    });
                    break;
                case "ShowDice":
                    RunOnUiThread(() =>
                    {
                        // Show the Dice view
                        FindViewById(Resource.Id.mv_PlayerMainView).Visibility = (ShowDice ? ViewStates.Gone : ViewStates.Visible);
                        FindViewById(Resource.Id.mv_PlayerDiceView).Visibility = (ShowDice ? ViewStates.Visible : ViewStates.Gone);
                        FindViewById(Resource.Id.mv_OpponentMainView).Visibility = (ShowDice ? ViewStates.Gone : ViewStates.Visible);
                        FindViewById(Resource.Id.mv_OpponentDiceView).Visibility = (ShowDice ? ViewStates.Visible : ViewStates.Gone);
                    });
                    break;
                case "PlayerDie":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_PlayerDiceValue, PlayerDie, PlayerDarkMode);
                        FindViewById(Resource.Id.mv_PlayerDiceValue).StartAnimation(DieChangeAnimation);
                    });
                    break;
                case "OpponentDie":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_OpponentDiceValue, OpponentDie, OpponentDarkMode);
                        FindViewById(Resource.Id.mv_OpponentDiceValue).StartAnimation(DieChangeAnimation);
                    });
                    break;
                case "PlayerDarkMode":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_PlayerDiceValue, PlayerDie, PlayerDarkMode);
                        FindViewById<ImageButton>(Resource.Id.mv_PlayerDecreaseHealth).SetImageResource(PlayerDarkMode ? Resource.Drawable.MinusButtonDark : Resource.Drawable.MinusButtonLight);
                        FindViewById<ImageButton>(Resource.Id.mv_PlayerIncreaseHealth).SetImageResource(PlayerDarkMode ? Resource.Drawable.PlusButtonDark : Resource.Drawable.PlusButtonLight);
                    });
                    break;
                case "OpponentDarkMode":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_OpponentDiceValue, OpponentDie, OpponentDarkMode);
                        FindViewById<ImageButton>(Resource.Id.mv_OpponentDecreaseHealth).SetImageResource(OpponentDarkMode ? Resource.Drawable.MinusButtonDark : Resource.Drawable.MinusButtonLight);
                        FindViewById<ImageButton>(Resource.Id.mv_OpponentIncreaseHealth).SetImageResource(OpponentDarkMode ? Resource.Drawable.PlusButtonDark : Resource.Drawable.PlusButtonLight);
                    });
                    break;
                case "DiceRollWinner":
                    RunOnUiThread(() =>
                    {
                        switch(DiceRollWinner)
                        {
                            case 1:
                                FindViewById(Resource.Id.mv_PlayerWinsDiceRoll).Visibility = ViewStates.Visible;
                                break;
                            case 2:
                                FindViewById(Resource.Id.mv_OpponentWinsDiceRoll).Visibility = ViewStates.Visible;
                                break;
                            default:
                                FindViewById(Resource.Id.mv_PlayerWinsDiceRoll).Visibility = ViewStates.Gone;
                                FindViewById(Resource.Id.mv_OpponentWinsDiceRoll).Visibility = ViewStates.Gone;
                                break;
                        }
                    });
                    break;
                case "PlayerBackground":
                    RunOnUiThread(() =>
                    {
                        FindViewById(Resource.Id.mv_PlayerLayout).SetBackgroundResource(PlayerBackground);
                        FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgSmp).SetImageResource(Resource.Drawable.swampManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgWtr).SetImageResource(Resource.Drawable.waterManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgPln).SetImageResource(Resource.Drawable.plainsManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgFst).SetImageResource(Resource.Drawable.forestManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgMtn).SetImageResource(Resource.Drawable.mountainManaUnselected);
                        switch (PlayerBackground)
                        {
                            case Resource.Drawable.swampBg:
                                FindViewById<ImageButton>(Resource.Id.mv_PlayerSelectBgSmp).SetImageResource(Resource.Drawable.swampMana);
                                break;
                            case Resource.Drawable.waterBg:
                                FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgWtr).SetImageResource(Resource.Drawable.waterMana);
                                break;
                            case Resource.Drawable.mountainBg:
                                FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgMtn).SetImageResource(Resource.Drawable.mountainMana);
                                break;
                            case Resource.Drawable.plainsBg:
                                FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgPln).SetImageResource(Resource.Drawable.plainsMana);
                                break;
                            case Resource.Drawable.forestBg:
                                FindViewById<ImageView>(Resource.Id.mv_PlayerSelectBgFst).SetImageResource(Resource.Drawable.forestMana);
                                break;
                        }
                    });
                    break;
                case "OpponentBackground":
                    RunOnUiThread(() =>
                    {
                        FindViewById(Resource.Id.mv_OpponentLayout).SetBackgroundResource(OpponentBackground);
                        FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgSmp).SetImageResource(Resource.Drawable.swampManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgWtr).SetImageResource(Resource.Drawable.waterManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgPln).SetImageResource(Resource.Drawable.plainsManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgFst).SetImageResource(Resource.Drawable.forestManaUnselected);
                        FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgMtn).SetImageResource(Resource.Drawable.mountainManaUnselected);
                        switch (OpponentBackground)
                        {
                            case Resource.Drawable.swampBg:
                                FindViewById<ImageButton>(Resource.Id.mv_OpponentSelectBgSmp).SetImageResource(Resource.Drawable.swampMana);
                                break;
                            case Resource.Drawable.waterBg:
                                FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgWtr).SetImageResource(Resource.Drawable.waterMana);
                                break;
                            case Resource.Drawable.mountainBg:
                                FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgMtn).SetImageResource(Resource.Drawable.mountainMana);
                                break;
                            case Resource.Drawable.plainsBg:
                                FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgPln).SetImageResource(Resource.Drawable.plainsMana);
                                break;
                            case Resource.Drawable.forestBg:
                                FindViewById<ImageView>(Resource.Id.mv_OpponentSelectBgFst).SetImageResource(Resource.Drawable.forestMana);
                                break;
                        }
                    });
                    break;
                case "ShowPlayerSelectBackgroundView":
                    RunOnUiThread(() =>
                    {
                        if (ShowPlayerSelectBackgroundView)
                        {
                            FindViewById(Resource.Id.mv_PlayerMainView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_PlayerDiceView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_PlayerSelectBgView).Visibility = ViewStates.Visible;
                        } else if(ShowDice)
                        {
                            FindViewById(Resource.Id.mv_PlayerMainView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_PlayerDiceView).Visibility = ViewStates.Visible;
                            FindViewById(Resource.Id.mv_PlayerSelectBgView).Visibility = ViewStates.Gone;
                        } else
                        {
                            FindViewById(Resource.Id.mv_PlayerMainView).Visibility = ViewStates.Visible;
                            FindViewById(Resource.Id.mv_PlayerDiceView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_PlayerSelectBgView).Visibility = ViewStates.Gone;
                        }
                    });
                    break;
                case "ShowOpponentSelectBackgroundView":
                    RunOnUiThread(() =>
                    {
                        if (ShowOpponentSelectBackgroundView)
                        {
                            FindViewById(Resource.Id.mv_OpponentMainView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_OpponentDiceView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_OpponentSelectBgView).Visibility = ViewStates.Visible;
                        }
                        else if (ShowDice)
                        {
                            FindViewById(Resource.Id.mv_OpponentMainView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_OpponentDiceView).Visibility = ViewStates.Visible;
                            FindViewById(Resource.Id.mv_OpponentSelectBgView).Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            FindViewById(Resource.Id.mv_OpponentMainView).Visibility = ViewStates.Visible;
                            FindViewById(Resource.Id.mv_OpponentDiceView).Visibility = ViewStates.Gone;
                            FindViewById(Resource.Id.mv_OpponentSelectBgView).Visibility = ViewStates.Gone;
                        }
                    });
                    break;
                 default:
                    Console.WriteLine($"{property} not bound");
                    break;
            }
        }

        #region View Management

        private void SetDieImage(int viewId, int value, bool darkMode)
        {
            int id = GetDiceImage(value, darkMode);
            if (id == 0)
                FindViewById(viewId).Visibility = ViewStates.Gone;
            else
            {
                var view = FindViewById<ImageView>(viewId);
                view.SetImageResource(id);
                view.Visibility = ViewStates.Visible;
            }
        }

        private void RollDice()
        {
            uint val = (uint)Math.Abs(Rand.Next());
            int pdie = 0;
            int odie = 0;

            for (uint i = 0; i < 4; i++)
            {
                if ((val & i) != 0) pdie++;
                if ((val & (i * 65536)) != 0) odie++;
            }

            PlayerDie = nextDieValue(PlayerDie, pdie);
            OpponentDie = nextDieValue(OpponentDie, odie);
        }

        private int nextDieValue(int currentDieValue, int randMod4)
        {
            // Each die face is opposite the face that when added together equals 7
            // ex: 1 opposite 6, 2 opposite 5, 3 opposite 4
            int[] allValues = new int[] { 1, 2, 3, 4, 5, 6 };
            int[] possibleValues = allValues.Except(new int[] { currentDieValue, 7 - currentDieValue }).ToArray();
            
            return possibleValues[randMod4];
        }

        private int GetDiceImage(int dieNumber, bool dark)
        {
            switch(dieNumber)
            {
                case 1:
                    return dark ? Resource.Drawable.Dice1Dark : Resource.Drawable.Dice1Light;
                case 2:
                    return dark ? Resource.Drawable.Dice2Dark : Resource.Drawable.Dice2Light;
                case 3:
                    return dark ? Resource.Drawable.Dice3Dark : Resource.Drawable.Dice3Light;
                case 4:
                    return dark ? Resource.Drawable.Dice4Dark : Resource.Drawable.Dice4Light;
                case 5:
                    return dark ? Resource.Drawable.Dice5Dark : Resource.Drawable.Dice5Light;
                case 6:
                    return dark ? Resource.Drawable.Dice6Dark : Resource.Drawable.Dice6Light;
                default:
                    return 0;
            }
        }

        private void UpdateHealth(int textViewId, int newValue)
        {
            var textView = FindViewById<TextView>(textViewId);
            textView.Text = newValue.ToString();
            textView.SetTextColor(new Color(GetColor(newValue > 0 ? Resource.Color.light_font : Resource.Color.red_font)));
        }

        #endregion View Management
    }
}

