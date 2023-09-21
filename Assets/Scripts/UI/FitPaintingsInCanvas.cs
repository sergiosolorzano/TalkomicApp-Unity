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

    public void AdjustGridLayout(GameObject[] targetGOs)
    {
        int numOfImages = targetGOs.Length;
        int maxRows = 3;
        float minWidth = 160f;

        float canvasWidth = targetCanvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = targetCanvas.GetComponent<RectTransform>().rect.height;

        // Initialize to max values
        int bestRows = maxRows;
        int bestColumns = Mathf.CeilToInt((float)numOfImages / maxRows);

        // If there are 9 or more images, set the bestRows to 3 and return
        if (numOfImages >= 9)
        {
            bestRows = 3;
            bestColumns = Mathf.CeilToInt((float)numOfImages / bestRows);
        }
        else
        {
            for (int rows = 1; rows <= maxRows; rows++)
            {
                // Calculate potential columns for this row configuration
                int columns = Mathf.CeilToInt((float)numOfImages / rows);

                float totalPaddingX = paddingXBetweenImages * (columns - 1);
                float cellWidth = (canvasWidth - totalPaddingX) / columns;

                // Check the width constraint
                if (cellWidth >= minWidth)
                {
                    bestRows = rows;
                    bestColumns = columns;
                    break;
                }
            }
        }

        // Calculate total padding for X and Y
        float totalPaddingXFinal = paddingXBetweenImages * (bestColumns - 1);
        float totalPaddingYFinal = paddingYBetweenImages * (bestRows - 1);

        // Calculate the final cell size
        float cellWidthFinal = (canvasWidth - totalPaddingXFinal) / bestColumns;
        float cellHeightFinal = (canvasHeight - totalPaddingYFinal) / bestRows;

        float cellSize = Mathf.Min(cellWidthFinal, cellHeightFinal);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridLayout.constraintCount = bestRows;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(paddingXBetweenImages, paddingYBetweenImages);
    }

}
