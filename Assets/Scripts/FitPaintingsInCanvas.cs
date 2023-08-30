using UnityEngine;
using UnityEngine.UI;

public class FitPaintingsInCanvas : MonoBehaviour
{
    public Canvas targetCanvas;
    public float paddingXBetweenImages = 10f;
    public float paddingYBetweenImages = 10f;
    public GridLayoutGroup gridLayout;

    private void Awake()
    {
        gridLayout = GameObject.FindWithTag("ImagePanel").GetComponent<GridLayoutGroup>();
    }

    public void AdjustGridLayout(int numOfImages, GameObject panel)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        float totalSpacing = gridLayout.spacing.x * (numOfImages - 1);
        float cellWidth = (panelRect.rect.width - totalSpacing) / numOfImages;

        // Assuming all images have the same height as the panel
        float cellHeight = panelRect.rect.height;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        gridLayout.spacing = new Vector2(paddingXBetweenImages, paddingYBetweenImages);
    }

    public void ScaleObjectToFitCanvas(GameObject[] targetGOs, int numOfImages)
    {
        float canvasWidth = targetCanvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = targetCanvas.GetComponent<RectTransform>().rect.height;

        int numOfRows = Mathf.CeilToInt(targetGOs.Length / (float)numOfImages);

        // Calculate total padding for X and Y
        float totalPaddingX = paddingXBetweenImages * (numOfImages - 1);
        float totalPaddingY = paddingYBetweenImages * (numOfRows - 1);

        // Subtract the total padding from the canvas width and height, then divide by the number of images or rows
        float imageWidth = (canvasWidth - totalPaddingX) / numOfImages;
        float imageHeight = (canvasHeight - totalPaddingY) / numOfRows;

        float squareSize = Mathf.Min(imageWidth, imageHeight);

        // Set cell size
        gridLayout.cellSize = new Vector2(squareSize, squareSize);

        // Set spacing
        gridLayout.spacing = new Vector2(paddingXBetweenImages, paddingYBetweenImages);
    }

}
