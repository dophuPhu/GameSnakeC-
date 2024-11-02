using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WMPLib;

public class SnakeGame : Form
{
    private List<Point> Snake = new List<Point>();
    private Point Food = new Point();
    private List<Point> Obstacles = new List<Point>(); // Danh sách chướng ngại vật
    private Random rand = new Random();
    private int score = 0;
    private int dirX, dirY;
    private bool isPaused = true;

    private WindowsMediaPlayer backgroundMusic = new WindowsMediaPlayer();

    private Timer gameTimer = new Timer();
    private PictureBox gameCanvas = new PictureBox();
    private Label lblScore = new Label();
    private ComboBox cbDifficulty;
    private ComboBox cbMode;
    private Button btnStartPause;
    private int obstacleCount = 5; // Số lượng chướng ngại vật ban đầu
    private int foodEaten = 0; // Số con mồi đã ăn

    public SnakeGame()
    {
        Text = "Snake Game";
        Size = new Size(800, 600);

        lblScore.Text = "Score: 0";
        lblScore.Location = new Point(10, 10);
        Controls.Add(lblScore);

        cbDifficulty = new ComboBox
        {
            Items = { "Easy", "Medium", "Khó" },
            Location = new Point(10, 40),
            DropDownStyle = ComboBoxStyle.DropDownList,
            SelectedIndex = 0
        };
        Controls.Add(cbDifficulty);

        cbMode = new ComboBox
        {
            Items = { "Classic", "Modern", "With Obstacles" }, // Thêm chế độ mới
            Location = new Point(10, 70),
            DropDownStyle = ComboBoxStyle.DropDownList,
            SelectedIndex = 0
        };
        cbMode.SelectedIndexChanged += (s, e) => NewGame(); // Đặt lại trò chơi khi chế độ thay đổi
        Controls.Add(cbMode);

        btnStartPause = new Button
        {
            Text = "Start",
            Location = new Point(10, 100)
        };
        btnStartPause.Click += (sender, e) =>
        {
            if (isPaused)
            {
                isPaused = false;
                btnStartPause.Text = "Pause";
                gameTimer.Start();
                PlayBackgroundMusic();
            }
            else
            {
                isPaused = true;
                btnStartPause.Text = "Start";
                gameTimer.Stop();
                backgroundMusic.controls.stop();
            }

            cbDifficulty.Enabled = isPaused;
            cbMode.Enabled = isPaused;
        };
        Controls.Add(btnStartPause);

        gameCanvas.Size = new Size(600, 400);
        gameCanvas.Location = new Point(100, 100);
        gameCanvas.BackColor = Color.Black; // Màu nền mặc định khi mở game
        gameCanvas.Paint += gameCanvas_Paint;
        Controls.Add(gameCanvas);

        gameTimer.Interval = 1000;
        gameTimer.Tick += Update;
        gameTimer.Stop();

        NewGame();
    }

    private void NewGame()
    {
        Snake.Clear();
        Snake.Add(new Point(10, 10));
        dirX = 1;
        dirY = 0;
        score = 0;
        foodEaten = 0; // Đặt lại số con mồi đã ăn
        lblScore.Text = "Score: " + score.ToString();

        PlaceFood();
        Obstacles.Clear(); // Xóa chướng ngại vật cũ
        if (cbMode.SelectedItem.ToString() == "With Obstacles")
        {
            PlaceObstacles();
        }
        UpdateDifficulty();
    }

    private void PlaceObstacles()
    {
        // Tạo chướng ngại vật
        for (int i = 0; i < obstacleCount; i++)
        {
            Point obstacle;
            do
            {
                obstacle = new Point(rand.Next(gameCanvas.Width / 10), rand.Next(gameCanvas.Height / 10));
            } while (Snake.Contains(obstacle) || obstacle.Equals(Food)); // Đảm bảo không trùng lặp với rắn hoặc thức ăn
            Obstacles.Add(obstacle);
        }
    }

    private void UpdateDifficulty()
    {
        switch (cbDifficulty.SelectedItem.ToString())
        {
            case "Easy": gameTimer.Interval = 200; break;
            case "Medium": gameTimer.Interval = 100; break;
            case "Khó": gameTimer.Interval = 50; break;
        }
    }

    private void PlayBackgroundMusic()
    {
        try
        {
            backgroundMusic.URL = "D:\\SnakeGame-main\\Chatgptgame_snake\\Chatgptgame_snake\\audio\\eat.mp3";
            backgroundMusic.settings.volume = 100;
            backgroundMusic.settings.setMode("loop", true);
            backgroundMusic.controls.play();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi phát nhạc: " + ex.Message);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        backgroundMusic.controls.stop();
        base.OnFormClosing(e);
    }

    private void PlaceFood()
    {
        Food = new Point(rand.Next(gameCanvas.Width / 10), rand.Next(gameCanvas.Height / 10));
    }

    private void Update(object sender, EventArgs e)
    {
        UpdateDifficulty();

        // Kiểm tra chế độ và cập nhật màu nền
        if (cbMode.SelectedItem.ToString() == "Classic")
        {
            gameCanvas.BackColor = Color.Black;
        }
        else
        {
            gameCanvas.BackColor = Color.DarkGray;
        }

        // Kiểm tra nếu rắn tự cắn vào thân
        for (int i = 1; i < Snake.Count; i++)
            if (Snake[i].Equals(Snake[0])) EndGame();

        // Kiểm tra va chạm với chướng ngại vật
        if (cbMode.SelectedItem.ToString() == "With Obstacles")
        {
            foreach (Point obstacle in Obstacles)
            {
                if (Snake[0].Equals(obstacle))
                {
                    EndGame(); // Kết thúc trò chơi khi rắn đụng vào chướng ngại vật
                    return; // Thoát khỏi phương thức để tránh thực hiện các lệnh khác
                }
            }
        }

        // Kiểm tra va chạm với thức ăn
        if (Snake[0].Equals(Food))
        {
            score++;
            lblScore.Text = "Score: " + score.ToString();
            Snake.Add(new Point());
            foodEaten++;
            PlaceFood();

            // Kiểm tra xem có cần tăng thêm chướng ngại vật không
            if (foodEaten % 7 == 0)
            {
                obstacleCount++; // Tăng số lượng chướng ngại vật
                PlaceObstacles(); // Tạo lại chướng ngại vật
            }
        }

        // Kiểm tra va chạm với viền ngoài (chế độ Classic)
        if (cbMode.SelectedItem.ToString() == "Classic")
        {
            if (Snake[0].X < 0 || Snake[0].Y < 0 || Snake[0].X >= gameCanvas.Width / 10 || Snake[0].Y >= gameCanvas.Height / 10)
                EndGame();
        }
        else
        {
            // Tạo hiệu ứng vòng lặp (Modern)
            if (Snake[0].X < 0) Snake[0] = new Point(gameCanvas.Width / 10 - 1, Snake[0].Y);
            else if (Snake[0].X >= gameCanvas.Width / 10) Snake[0] = new Point(0, Snake[0].Y);
            else if (Snake[0].Y < 0) Snake[0] = new Point(Snake[0].X, gameCanvas.Height / 10 - 1);
            else if (Snake[0].Y >= gameCanvas.Height / 10) Snake[0] = new Point(Snake[0].X, 0);
        }

        // Cập nhật vị trí các phần của rắn
        for (int i = Snake.Count - 1; i >= 0; i--)
        {
            if (i == 0)
                Snake[i] = new Point(Snake[i].X + dirX, Snake[i].Y + dirY);
            else
                Snake[i] = Snake[i - 1];
        }

        gameCanvas.Invalidate();
    }


    private void EndGame()
    {
        gameTimer.Stop();
        isPaused = true;
        btnStartPause.Text = "Start";
        cbDifficulty.Enabled = true;
        cbMode.Enabled = true;
        MessageBox.Show("Thua rồi gà quá !");
        backgroundMusic.controls.stop();
        NewGame();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (!isPaused)
        {
            switch (keyData)
            {
                case Keys.Up:
                    if (dirY != 1) { dirX = 0; dirY = -1; }
                    break;
                case Keys.Down:
                    if (dirY != -1) { dirX = 0; dirY = 1; }
                    break;
                case Keys.Left:
                    if (dirX != 1) { dirX = -1; dirY = 0; }
                    break;
                case Keys.Right:
                    if (dirX != -1) { dirX = 1; dirY = 0; }
                    break;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void gameCanvas_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        // Vẽ rắn
        foreach (Point p in Snake)
        {
            g.FillRectangle(Brushes.Green, p.X * 10, p.Y * 10, 10, 10);
        }

        // Vẽ thức ăn
        g.FillRectangle(Brushes.Red, Food.X * 10, Food.Y * 10, 10, 10);

        // Vẽ chướng ngại vật
        if (cbMode.SelectedItem.ToString() == "With Obstacles")
        {
            foreach (Point obstacle in Obstacles)
            {
                g.FillRectangle(Brushes.Blue, obstacle.X * 10, obstacle.Y * 10, 10, 10);
            }
        }
    }

    //[STAThread]
    //public static void Main()
    //{
    //    Application.Run(new SnakeGame());
    //}
}
