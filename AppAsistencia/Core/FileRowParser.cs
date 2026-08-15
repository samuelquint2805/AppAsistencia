namespace AppAsistencia.Core
{
    public static class FileRowParser
    {
        public static List<Dictionary<string, string>> ExtraerFilas(Stream archivo, string nombreArchivo, List<string> errores)
        {
            var filas = new List<Dictionary<string, string>>();
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

            if (extension == ".csv")
            {
                using var lector = new StreamReader(archivo);
                var encabezado = lector.ReadLine()?.Split(',').Select(h => h.Trim()).ToArray();
                if (encabezado is null) return filas;

                string? linea;
                while ((linea = lector.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var valores = linea.Split(',');
                    var fila = new Dictionary<string, string>();
                    for (int i = 0; i < encabezado.Length && i < valores.Length; i++)
                        fila[encabezado[i]] = valores[i].Trim();
                    filas.Add(fila);
                }
            }
            else if (extension == ".xlsx")
            {
                using var libro = new ClosedXML.Excel.XLWorkbook(archivo);
                var hoja = libro.Worksheet(1);
                var encabezados = hoja.Row(1).CellsUsed().Select(c => c.GetString().Trim()).ToList();

                foreach (var filaExcel in hoja.RowsUsed().Skip(1))
                {
                    var fila = new Dictionary<string, string>();
                    for (int i = 0; i < encabezados.Count; i++)
                        fila[encabezados[i]] = filaExcel.Cell(i + 1).GetString().Trim();
                    filas.Add(fila);
                }
            }
            else
            {
                errores.Add("Formato de archivo no soportado. Usa .csv o .xlsx");
            }

            return filas;
        }

        // Mapea nombres de dia en español/ingles al enum DayOfWeek de .NET
        public static DayOfWeek? ParsearDiaSemana(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            return texto.Trim().ToLowerInvariant() switch
            {
                "lunes" or "monday" => DayOfWeek.Monday,
                "martes" or "tuesday" => DayOfWeek.Tuesday,
                "miercoles" or "miércoles" or "wednesday" => DayOfWeek.Wednesday,
                "jueves" or "thursday" => DayOfWeek.Thursday,
                "viernes" or "friday" => DayOfWeek.Friday,
                "sabado" or "sábado" or "saturday" => DayOfWeek.Saturday,
                "domingo" or "sunday" => DayOfWeek.Sunday,
                _ => null
            };
        }

        public static string NombreDiaEnEspanol(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => dia.ToString()
        };
    }
}
