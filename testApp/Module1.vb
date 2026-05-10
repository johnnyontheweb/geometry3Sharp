Module Module1

    Sub Main()
        ' Crea una mesh triangolata del rettangolo con vincoli di area
        Dim minArea As Double = 0.5
        Dim maxArea As Double = 2.0
        Dim mesh = CreateConstrainedRectangleMesh(0, 0, 10, 0, 10, 10, 0, 10, minArea, maxArea)

        ' Visualizza i risultati
        Console.WriteLine($"Numero di vertici: {mesh.VertexCount}")
        Console.WriteLine($"Numero di triangoli: {mesh.TriangleCount}")
        Console.WriteLine("")

        ' Calcola aree dei triangoli
        Dim areas As New List(Of Double)()
        For i = 0 To mesh.TriangleCount - 1
            Dim area = mesh.GetTriArea(i)
            areas.Add(area)
            Console.WriteLine($"Triangolo {i}: area = {area:F4}")
        Next

        Console.WriteLine("")
        Console.WriteLine($"Area minima: {areas.Min():F4}")
        Console.WriteLine($"Area massima: {areas.Max():F4}")
        Console.WriteLine($"Area media: {areas.Average():F4}")
    End Sub

    Function CreateConstrainedRectangleMesh(x1 As Double, y1 As Double, x2 As Double, y2 As Double,
                                            x3 As Double, y3 As Double, x4 As Double, y4 As Double,
                                            minArea As Double, maxArea As Double) As g3.DMesh3
        ' Crea un poligono rettangolare usando Polygon2d
        Dim outer = New g3.Polygon2d()
        outer.AppendVertex(New g3.Vector2d(x1, y1))
        outer.AppendVertex(New g3.Vector2d(x2, y2))
        outer.AppendVertex(New g3.Vector2d(x3, y3))
        outer.AppendVertex(New g3.Vector2d(x4, y4))

        ' Crea GeneralPolygon2d dal Polygon2d
        Dim polygon = New g3.GeneralPolygon2d(outer)

        ' Usa TriangulatedPolygonGenerator per creare una mesh iniziale
        Dim polyGen = New g3.TriangulatedPolygonGenerator With {
            .Polygon = polygon,
            .Subdivisions = 4
        }
        polyGen.Generate()

        Dim mesh As g3.DMesh3 = New g3.DMesh3()
        polyGen.MakeMesh(mesh)

        ' Applica il Remesher con vincoli su area
        Dim remesher = New g3.Remesher(mesh)

        ' Calcola la lunghezza target dell'edge basata sull'area
        ' area = sqrt(3)/4 * edge^2, quindi edge = sqrt(4*area/sqrt(3))
        Dim targetEdgeLengthMin As Double = Math.Sqrt(4 * minArea / Math.Sqrt(3))
        Dim targetEdgeLengthMax As Double = Math.Sqrt(4 * maxArea / Math.Sqrt(3))

        remesher.MinEdgeLength = targetEdgeLengthMin
        remesher.MaxEdgeLength = targetEdgeLengthMax

        remesher.EnableFlips = True
        remesher.EnableCollapses = True
        remesher.EnableSplits = True
        remesher.EnableSmoothing = True

        remesher.Precompute()

        ' Esegui il remeshing per diverse iterazioni
        For i = 0 To 10
            remesher.BasicRemeshPass()
        Next

        Return mesh
    End Function

End Module