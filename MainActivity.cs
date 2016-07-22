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

        private Animation ResetHealthAnimation
        {
            get
            {
                var anim = new ShakeAnimation(64.0f);
                anim.Duration = 300;
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
            FindViewById<Button>(Resource.Id.mv_OpponentIncreaseHealth).Click += IncreaseOpponentHealthCommand;
            FindViewById<Button>(Resource.Id.mv_OpponentDecreaseHealth).Click += DecreaseOpponentHealthCommand;
            FindViewById<Button>(Resource.Id.mv_PlayerIncreaseHealth).Click += IncreasePlayerHealthCommand;
            FindViewById<Button>(Resource.Id.mv_PlayerDecreaseHealth).Click += DecreasePlayerHealthCommand;

            // Menu Open/Close
            FindViewById<Button>(Resource.Id.mv_menuToggle).Click += ToggleMenuCommand;
            FindViewById<Button>(Resource.Id.mv_menuClose).Click += CloseMenuCommand;
            
            // Menu Buttons
            FindViewById<Button>(Resource.Id.mv_menu_reset).Click += ResetCommand;
            FindViewById<Button>(Resource.Id.mv_menu_dice).Click += DiceCommand;
            FindViewById<Button>(Resource.Id.mv_menu_life).Click += LifeCommand;
            FindViewById<Button>(Resource.Id.mv_menu_settings).Click += SettingsCommand;

            // Hide nav buttons
            FindViewById(Resource.Id.mv_LayoutRoot).SystemUiVisibility = StatusBarVisibility.Hidden;

            PlayerBackground = Resource.Drawable.plainsBg;
            OpponentBackground = Resource.Drawable.swampBg;
        }

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

        private void ToggleMenuCommand(object sender, EventArgs args)
        {
            ShowMenu = !ShowMenu;
        }
        private void CloseMenuCommand(object sender, EventArgs args)
        {
            ShowMenu = false;
        }

        private void ResetCommand(object sender, EventArgs args)
        {
            PlayerHealth = BaseHealth;
            OpponentHealth = BaseHealth;
            RunOnUiThread(() =>
            {
                FindViewById(Resource.Id.mv_PlayerHealth).StartAnimation(ResetHealthAnimation);
                FindViewById(Resource.Id.mv_OpponentHealth).StartAnimation(ResetHealthAnimation);
                var timer = new Timer();
                timer.Interval = 500;
                timer.Elapsed += delegate
                {
                    timer.Stop();
                    ShowMenu = false;
                    timer.Dispose();
                };
                timer.Start();
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

        }
        private void SetLife20Command(object sender, EventArgs args)
        {
            BaseHealth = 20;
            ResetCommand(sender, args);
        }
        private void SetLife30Command(object sender, EventArgs args)
        {
            BaseHealth = 30;
            ResetCommand(sender, args);
        }
        private void SetLife40Command(object sender, EventArgs args)
        {
            BaseHealth = 40;
            ResetCommand(sender, args);
        }

        private void SettingsCommand(object sender, EventArgs args)
        {

        }

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
                    });
                    break;
                case "OpponentDie":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_OpponentDiceValue, OpponentDie, OpponentDarkMode);
                    });
                    break;
                case "PlayerDarkMode":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_PlayerDiceValue, PlayerDie, PlayerDarkMode);
                        FindViewById<Button>(Resource.Id.mv_PlayerDecreaseHealth).SetCompoundDrawables(GetDrawable(PlayerDarkMode ? Resource.Drawable.MinusButtonDark : Resource.Drawable.MinusButtonLight), null, null, null);
                        FindViewById<Button>(Resource.Id.mv_PlayerIncreaseHealth).SetCompoundDrawables(null, null, GetDrawable(PlayerDarkMode ? Resource.Drawable.PlusButtonDark : Resource.Drawable.PlusButtonLight), null);
                    });
                    break;
                case "OpponentDarkMode":
                    RunOnUiThread(() =>
                    {
                        SetDieImage(Resource.Id.mv_OpponentDiceValue, OpponentDie, OpponentDarkMode);
                        FindViewById<Button>(Resource.Id.mv_OpponentDecreaseHealth).SetCompoundDrawables(GetDrawable(OpponentDarkMode ? Resource.Drawable.MinusButtonDark : Resource.Drawable.MinusButtonLight), null, null, null);
                        FindViewById<Button>(Resource.Id.mv_OpponentIncreaseHealth).SetCompoundDrawables(null, null, GetDrawable(OpponentDarkMode ? Resource.Drawable.PlusButtonDark : Resource.Drawable.PlusButtonLight), null);
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
                    });
                    break;
                case "OpponentBackground":
                    RunOnUiThread(() =>
                    {
                        FindViewById(Resource.Id.mv_OpponentLayout).SetBackgroundResource(OpponentBackground);
                    });
                    break;
                 default:
                    Console.WriteLine($"{property} not bound");
                    break;
            }
        }

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
            int pdie = 1;
            int odie = 1;

            for (uint i = 0; i < 5; i++)
            {
                if ((val & i) != 0) pdie++;
                if ((val & (i * 65536)) != 0) odie++;
            }

            PlayerDie = pdie;
            OpponentDie = odie;
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
    }
}

