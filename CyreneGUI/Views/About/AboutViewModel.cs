using System.Collections.ObjectModel;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CyreneGUI.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace CyreneGUI.Views.About;

public sealed partial class AboutViewModel : ObservableObject
{
    [ObservableProperty] public partial Visibility AuthorVisibility { get; set; }
    private readonly ObservableCollection<(Image Img, int Id, double DepthOffset)> GalleryItems = [];
    private double CameraZ = 0.0;
    private const int ImageCount = 6;
    private const double CycleLen = 4.5;
    private const double BaseRadius = 550;
    private const double ScrollStep = 0.05;
    private readonly ObservableCollection<Vector2> Directions =
    [
        new(-1, 0),
        new(-0.7f, -0.7f),
        new(0, -1),
        new(0.7f, -0.7f),
        new(1, 0),
        new(0.7f, 0.7f),
        new(0, 1),
        new(-0.7f, 0.7f)
    ];

    public void RefreshVisibility()
    {
        AuthorVisibility = AppConfigUtil.Config.UI.Puzzle == PuzzleState.SYAFinished
            ? Visibility.Visible : Visibility.Collapsed;
    }

    public void InitGallery(Canvas galleryCanvas)
    {
        GalleryItems.Clear();
        galleryCanvas.Children.Clear();

        // Random offsets
        var offsets = Enumerable.Range(0, ImageCount).Select(x => x * CycleLen / ImageCount).ToList();
        var n = offsets.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Shared.Next(n + 1);
            (offsets[k], offsets[n]) = (offsets[n], offsets[k]);
        }

        for (int i = 1; i <= ImageCount; i++)
        {
            var img = new Image
            {
                Source = AppUtil.GetImage($"1415_Rank_{i}.png"),
                Width = 550,
                Height = 550,
                Opacity = 0,
                IsHitTestVisible = false,
                RenderTransform = new CompositeTransform()
            };
            galleryCanvas.Children.Add(img);
            GalleryItems.Add((img, i, offsets[i - 1]));
        }
    }

    public void HandleScroll(double delta)
    {
        CameraZ = delta > 0 ? CameraZ - ScrollStep : CameraZ + ScrollStep;
    }

    public void UpdateGalleryPositions(double actualWidth, double actualHeight)
    {
        var centerX = actualWidth / 2;
        var centerY = actualHeight / 2;
        if (centerX == 0 || centerY == 0) return;

        foreach (var (img, id, offset) in GalleryItems)
        {
            if (img.RenderTransform is not CompositeTransform transform) continue;

            // Direction
            var totalZ = CameraZ + offset;
            var rnd = new Random((int)Math.Floor(totalZ / CycleLen));
            var shuffled = Enumerable.Range(0, Directions.Count).OrderBy(x => rnd.Next()).ToList();
            int dirIndex = shuffled[(id - 1) % Directions.Count];
            var dir = Directions[dirIndex];

            // Scale
            var z = totalZ % CycleLen;
            if (z < 0) z += CycleLen;
            var progress = Math.Min(Math.Max(0.0, z / 1.0), 1.0);
            var scale = 1.0 - Math.Pow(1.0 - progress, 3); // Ease out formula
            transform.ScaleX = scale;
            transform.ScaleY = scale;

            // Position
            var distance = z * BaseRadius;
            transform.TranslateX = centerX + (dir.X * distance) - (img.Width / 2);
            transform.TranslateY = centerY + (dir.Y * distance) - (img.Height / 2);
            transform.CenterX = img.Width / 2;
            transform.CenterY = img.Height / 2;

            // Opacity
            var fade = z < 0.5 ? z / 0.5 : 1.0;
            fade = Math.Min(Math.Max(0.0, fade), 1.0);
            img.Opacity = 0.75 * fade;
        }
    }
}