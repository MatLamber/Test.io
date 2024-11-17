using UnityEngine;

public class InfiniteGrid : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] quadTransforms; // Array de transformaciones de cuadrantes
    private Transform[,] quads; // Matriz de cuadrantes
    private Vector2Int playerLastGridPosition;
    private float quadSize; // Tamaño del cuadrante basado en el collider
    private int gridSize; // Determinado dinámicamente

    void Start()
    {
        if (quadTransforms == null || quadTransforms.Length == 0)
        {
            Debug.LogError(
                "Cuadrantes no están asignados. Asegúrate de asignar los cuadrantes presentes en la escena.");
            return;
        }

        // Calcular dinámicamente el tamaño de la cuadrícula
        CalculateGridSize();

        CalculateQuadSize();
        InitializeGrid();
        playerLastGridPosition = GetPlayerGridPosition();
    }

    void Update()
    {
        Vector2Int playerCurrentGridPosition = GetPlayerGridPosition();
        if (playerCurrentGridPosition != playerLastGridPosition)
        {
            RepositionQuads(playerCurrentGridPosition);
            playerLastGridPosition = playerCurrentGridPosition;
        }
    }

    void CalculateGridSize()
    {
        gridSize = Mathf.CeilToInt(Mathf.Sqrt(quadTransforms.Length));
        if (gridSize % 2 == 0)
        {
            gridSize += 1; // Asegúrate de que el tamaño de la cuadrícula sea impar
        }
    }

    void CalculateQuadSize()
    {
        if (quadTransforms.Length > 0)
        {
            Collider quadCollider = quadTransforms[0].GetComponent<Collider>();
            if (quadCollider != null)
            {
                quadSize = quadCollider.bounds.size.x; // Suponiendo que el cuadrante es simétrico y X=Z
            }
            else
            {
                Debug.LogError("El primer cuadrante no tiene un Collider.");
            }
        }
        else
        {
            Debug.LogError("La lista de transformaciones de cuadrantes está vacía.");
        }
    }

    void InitializeGrid()
    {
        if (quadTransforms.Length != gridSize * gridSize)
        {
            Debug.LogError(
                "La cantidad de cuadrantes no coincide con el tamaño de la cuadrícula. Asegúrate de tener suficientes cuadrantes.");
            return;
        }

        quads = new Transform[gridSize, gridSize];

        // Obtener la posición del primer cuadrante en la lista
        Vector3 centeringOffset = quadTransforms[0].position;

        int index = 0;
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                quads[x, z] = quadTransforms[index];

                // Colocar cada cuadrante en la posición correcta basándose en el cuadrante central
                float posX = centeringOffset.x + (x - gridSize / 2) * quadSize;
                float posZ = centeringOffset.z + (z - gridSize / 2) * quadSize;
                quads[x, z].position = new Vector3(posX, quads[x, z].position.y, posZ);

                index++;
            }
        }
    }

    Vector2Int GetPlayerGridPosition()
    {
        return new Vector2Int(
            Mathf.FloorToInt(player.position.x / quadSize),
            Mathf.FloorToInt(player.position.z / quadSize)
        );
    }

    void RepositionQuads(Vector2Int playerGridPosition)
    {
        int dx = playerGridPosition.x - playerLastGridPosition.x;
        int dz = playerGridPosition.y - playerLastGridPosition.y;

        if (dx != 0)
        {
            RepositionColumns(dx);
        }

        if (dz != 0)
        {
            RepositionRows(dz);
        }
    }

    void RepositionColumns(int dx)
    {
        if (dx > 0) // Movimiento a la derecha
        {
            var firstColumn = new Transform[gridSize];
            for (int z = 0; z < gridSize; z++)
            {
                firstColumn[z] = quads[0, z];
            }

            for (int x = 0; x < gridSize - 1; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    quads[x, z] = quads[x + 1, z];
                }
            }

            for (int z = 0; z < gridSize; z++)
            {
                quads[gridSize - 1, z] = firstColumn[z];
                quads[gridSize - 1, z].position += Vector3.right * gridSize * quadSize;
            }
        }
        else if (dx < 0) // Movimiento a la izquierda
        {
            var lastColumn = new Transform[gridSize];
            for (int z = 0; z < gridSize; z++)
            {
                lastColumn[z] = quads[gridSize - 1, z];
            }

            for (int x = gridSize - 1; x > 0; x--)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    quads[x, z] = quads[x - 1, z];
                }
            }

            for (int z = 0; z < gridSize; z++)
            {
                quads[0, z] = lastColumn[z];
                quads[0, z].position -= Vector3.right * gridSize * quadSize;
            }
        }
    }

    void RepositionRows(int dz)
    {
        if (dz > 0) // Movimiento hacia adelante
        {
            var firstRow = new Transform[gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                firstRow[x] = quads[x, 0];
            }

            for (int z = 0; z < gridSize - 1; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    quads[x, z] = quads[x, z + 1];
                }
            }

            for (int x = 0; x < gridSize; x++)
            {
                quads[x, gridSize - 1] = firstRow[x];
                quads[x, gridSize - 1].position += Vector3.forward * gridSize * quadSize;
            }
        }
        else if (dz < 0) // Movimiento hacia atrás
        {
            var lastRow = new Transform[gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                lastRow[x] = quads[x, gridSize - 1];
            }

            for (int z = gridSize - 1; z > 0; z--)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    quads[x, z] = quads[x, z - 1];
                }
            }

            for (int x = 0; x < gridSize; x++)
            {
                quads[x, 0] = lastRow[x];
                quads[x, 0].position -= Vector3.forward * gridSize * quadSize;
            }
        }
    }
}