//David Rubio Moreno y Antonio Morales Pérez
using System.Diagnostics.CodeAnalysis;

namespace FP2_Practica1
{
    internal class Program
    {
        const bool DEBUG = false; // para sacar información adicional en el Render

        const int ANCHO = 25, ALTO = 16,  // área de juego
                   MAX_BALAS = 5, MAX_ENEMIGOS = 9;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            InicializaEntidades(out Entidad nave, out GrEntidades enemigos, out GrEntidades balas, out GrEntidades colisiones);//Inicializas todas las entidades
            IniciaTunel(out Tunel tunel);
            Console.WriteLine("Cargar Partida: 1");
            string carga = Console.ReadLine();
            if (carga == "1")
            {
                CargarPartida(ref enemigos, ref tunel, ref nave);//Carga si pulsas 1
            }
            while (!ChoqueNave(nave))
            {
                EliminaColisiones(ref colisiones);
                char ch = LeeInput();
                if (ch == 'q') { GuardarPartida(enemigos, tunel, nave); }//Guarda partida
                AvanzaTunel(ref tunel);
                GeneraEnemigo(ref enemigos, tunel);
                AvanzaEnemigos(ref enemigos, tunel);
                Colisiones(ref tunel, ref nave, ref balas, ref enemigos, ref colisiones);
                AvanzaNave(ch, ref nave);
                if (ch == 'x') { GeneraBala(ref balas, nave); }//Si pulsas X dispara
                AvanzaBalas(ref balas);
                Colisiones(ref tunel, ref nave, ref balas, ref enemigos, ref colisiones);
                if (!ChoqueNave(nave))
                    Render(tunel, nave, enemigos, balas, colisiones);
                Thread.Sleep(100);
            }

            Console.SetCursorPosition(0, ALTO + 1);
        }
        static Random rnd = new Random(); // un único generador de aleaotorios para todo el programa

        struct Tunel
        {
            public int[] suelo, techo;
            public int ini;
        }

        struct Entidad
        {
            public int fil, col;
        }

        struct GrEntidades
        {
            public Entidad[] ent;
            public int num;
        }
        static void ColNaveTunel(Tunel tunel, ref Entidad nave, ref GrEntidades colisiones)
        {
            int calculoColumna;
            calculoColumna = Math.Abs((nave.col + tunel.ini + 1) % ANCHO);
            if (nave.fil >= tunel.suelo[calculoColumna] || nave.fil <= tunel.techo[calculoColumna])//Si esta por debajo del suelo o encima del techo se choca
            {
                colisiones.ent[colisiones.num].fil = nave.fil;
                colisiones.ent[colisiones.num].col = nave.col;
                colisiones.num++;
                nave.fil = -1;
                nave.col = -1;
            }

        }
        static void ColBalastunel(ref Tunel tunel, ref GrEntidades balas, ref GrEntidades colisiones)
        {
            int calculoColumna, i = 0;
            bool aumentaCont = true;

            while (i < balas.num)
            {
                aumentaCont = true;
                calculoColumna = Math.Abs((balas.ent[i].col + tunel.ini + 1) % ANCHO);
                if (balas.ent[i].fil == tunel.suelo[calculoColumna])//Si la bala esta en menor o igual altura del tunel lo rompe
                {
                    EliminaEntidad(i, ref balas);
                    tunel.suelo[calculoColumna]++;//aumentas el suelo disminuyendo en pantalla
                    aumentaCont = false;
                }
                else if (balas.ent[i].fil == tunel.techo[calculoColumna])
                {
                    tunel.techo[calculoColumna] = balas.ent[i].fil - 1;//Cortas el techo
                    EliminaEntidad(i, ref balas);
                    aumentaCont = false;
                }
                if (aumentaCont) i++;//Si no ha detectado colision pasa a la siguiente pero en caso contrario mira la bala que ha sido posicionada al eliminar

            }
        }
        static void ColNaveEnemigos(ref Entidad nave, ref GrEntidades enemigos, ref GrEntidades colisiones)
        {
            for (int i = 0; i < enemigos.num; i++)
            {
                if (nave.fil == enemigos.ent[i].fil && nave.col == enemigos.ent[i].col)//Si la nave y el enemigo tienen las mismas coordenadas pierdes
                {
                    colisiones.ent[colisiones.num].fil = enemigos.ent[i].fil;//Se crea colision
                    colisiones.ent[colisiones.num].col = enemigos.ent[i].col;
                    colisiones.num++;
                    nave.fil = -1;
                    nave.col = -1;
                    EliminaEntidad(i, ref enemigos);
                }
            }
        }
        static void ColBalasEnemigos(ref GrEntidades balas, ref GrEntidades enemigos, ref GrEntidades colisiones)
        {
            int i = 0, j = 0;
            bool avanza = true;
            while (i < balas.num)
            {
                avanza = true;
                while (j < enemigos.num)
                {
                    if (balas.ent[i].fil == enemigos.ent[j].fil && balas.ent[i].col == enemigos.ent[j].col) //Si estan en la misma posicion se destruyen
                    {
                        colisiones.ent[colisiones.num].fil = enemigos.ent[j].fil;
                        colisiones.ent[colisiones.num].col = enemigos.ent[j].col;
                        colisiones.num++;
                        EliminaEntidad(j, ref enemigos);
                        EliminaEntidad(i, ref balas);
                        j = 0;//Cuando un enemigo se ha destuido se comienza a mirar desde el primero para la nueva bala
                    }
                    else j++;
                }
                i++;
            }
        }
        static void EliminaColisiones(ref GrEntidades colisiones)
        {
            colisiones.num = 0;
        }
        static void Colisiones(ref Tunel tunel, ref Entidad nave, ref GrEntidades balas, ref GrEntidades enemigos, ref GrEntidades colisiones)
        {
            if (!ChoqueNave(nave))//Si la nave se estrello en la primera comprobacion, no vuelve a comprobar
            {
                ColNaveTunel(tunel, ref nave, ref colisiones);//Nave tunel
                ColBalastunel(ref tunel, ref balas, ref colisiones);//Bala tunel
                ColNaveEnemigos(ref nave, ref enemigos, ref colisiones);//Nave enemigo
                ColBalasEnemigos(ref balas, ref enemigos, ref colisiones);
            }
        }
        static bool ChoqueNave(Entidad nave)
        {
            bool terminado = false;//Detecta si te has estrellado
            if (nave.fil == -1 || nave.col == -1) { terminado = true; }
            return terminado;
        }
        static void GeneraBala(ref GrEntidades balas, Entidad nave)
        {
            if (balas.num < MAX_BALAS && nave.col < ANCHO)
            {
                Entidad bal = new Entidad();//Crea una entidad nueva
                bal.fil = nave.fil;
                bal.col = nave.col;
                AñadeEntidad(bal, ref balas);
            }
        }
        static void GeneraEnemigo(ref GrEntidades enemigos, Tunel tunel)
        {
            int genera = rnd.Next(0, 4);
            if (enemigos.num < MAX_ENEMIGOS && genera == 0)// Genera un enemigo con 1/4 de probabilidad
            {
                Entidad ent = new Entidad();
                ent.col = ANCHO;
                ent.fil = rnd.Next(tunel.techo[(tunel.ini - 1 + ANCHO) % ANCHO] + 1, tunel.suelo[(tunel.ini - 1 + ANCHO) % ANCHO] - 1);//Calcula la fila
                AñadeEntidad(ent, ref enemigos);

            }
        }
        static void AñadeEntidad(Entidad ent, ref GrEntidades gr)
        {
            gr.ent[gr.num] = ent;
            gr.num++;
        }
        static void AvanzaBalas(ref GrEntidades balas)
        {
            for (int i = 0; i < balas.num; i++)
            {
                if (balas.ent[i].col == ANCHO - 1)//Si no han llegado al final avanzan
                {
                    EliminaEntidad(i, ref balas);
                }
                balas.ent[i].col++;
            }
        }
        static void AvanzaEnemigos(ref GrEntidades enemigos, Tunel tunel)
        {
            for (int i = 0; i < enemigos.num; i++)
            {
                if (enemigos.ent[i].col == 0)
                {
                    EliminaEntidad(i, ref enemigos);//Si no han llegado al final avanzan
                }
                enemigos.ent[i].col--;
            }
        }
        static void GuardarPartida(GrEntidades enemigos, Tunel tunel, Entidad nave)
        {
            StreamWriter guarda = new StreamWriter("nave.txt");
            guarda.WriteLine(tunel.ini);//Tunel ini
            guarda.WriteLine(ANCHO);
            guarda.WriteLine(enemigos.num);//Numero enenigos
            for (int i = 0; i < enemigos.num; i++)//Info enemigo
            {
                guarda.WriteLine(enemigos.ent[i].fil + enemigos.ent[i].col);
            }
            guarda.WriteLine(nave.fil);//Nave fil           
            guarda.WriteLine(nave.col);//Nave col
            for (int i = 0; i < ANCHO; i++)//Techo
            {
                guarda.WriteLine(tunel.techo[i]);
            }
            for (int i = 0; i < ANCHO; i++)//Suelo
            {
                guarda.WriteLine(tunel.suelo[i]);
            }
            guarda.Close();


        }
        static void CargarPartida(ref GrEntidades enemigos, ref Tunel tunel, ref Entidad nave)
        {
            StreamReader carga = new StreamReader("nave.txt");//Se lee la info del archivo
            tunel.ini = int.Parse(carga.ReadLine());
            int longTunel = int.Parse(carga.ReadLine());
            enemigos.num = int.Parse(carga.ReadLine());

            for (int i = 0; i < enemigos.num; i++)
            {
                string enemigoPos = carga.ReadLine();
                enemigos.ent[i].fil = int.Parse(enemigoPos[0].ToString());
                enemigos.ent[i].col = int.Parse(enemigoPos[1].ToString());
            }
            nave.fil = int.Parse(carga.ReadLine());
            nave.col = int.Parse(carga.ReadLine());
            for (int i = 0; i < longTunel; i++)
            {
                tunel.techo[(tunel.ini + i + 1) % ANCHO] = int.Parse(carga.ReadLine());
            }
            for (int i = 0; i < longTunel; i++)
            {
                tunel.suelo[(tunel.ini + i + 1) % ANCHO] = int.Parse(carga.ReadLine());
            }
            carga.Close();
        }

        static void RenderTunel(Tunel tunel)
        {
            for (int i = 0; i < ANCHO; i++)
            {
                for (int j = 0; j < ALTO; j++)//Comprueba por columnas si es mayor o menor y renderiza con su color
                {
                    if (j <= tunel.techo[tunel.ini] || j >= tunel.suelo[tunel.ini])
                        Console.BackgroundColor = ConsoleColor.Blue;
                    else Console.BackgroundColor = ConsoleColor.Black;
                    Console.SetCursorPosition(2 * i, j);
                    Console.Write("  ");

                }
                tunel.ini = (tunel.ini + 1) % ANCHO;//Aumenta ini
            }
            if (DEBUG)
            {
                Console.SetCursorPosition(0, ALTO + 1);
                Console.Write("  ");
                Console.SetCursorPosition(0, ALTO + 1);
                Console.Write(tunel.ini);//Ini
            }
        }
        static void AvanzaNave(char ch, ref Entidad nave)
        {
            switch (ch)
            {
                case 'l':
                    if (nave.col > 0)//Avanza izquierda
                        nave.col--;
                    break;
                case 'r':
                    if (nave.col < ANCHO - 1)//avanza
                        nave.col++;
                    break;
                case 'u':
                    if (nave.fil > 0)//Baja
                        nave.fil--;
                    break;
                case 'd':
                    if (nave.fil < ALTO - 1)//Sube
                        nave.fil++;
                    break;

                default:
                    break;
            }

        }
        static void Render(Tunel tunel, Entidad nave, GrEntidades enemigos, GrEntidades balas, GrEntidades colisiones)
        {

            RenderTunel(tunel);
            Console.BackgroundColor = ConsoleColor.Black;
            if (nave.col > -1)//Nave
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(nave.col * 2, nave.fil);
                Console.Write("=>");
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            for (int i = 0; i < enemigos.num; i++)//Enemigos
            {
                Console.SetCursorPosition(enemigos.ent[i].col * 2, enemigos.ent[i].fil);
                Console.Write("<>");
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            for (int i = 0; i < balas.num; i++)//Balas
            {
                Console.SetCursorPosition(balas.ent[i].col * 2, balas.ent[i].fil);
                Console.Write("--");
            }
            Console.ForegroundColor = ConsoleColor.Red;
            for (int i = 0; i < colisiones.num; i++)//Colisiones
            {
                Console.SetCursorPosition(colisiones.ent[i].col * 2, colisiones.ent[i].fil);
                Console.Write("**");
            }
            if (DEBUG)
            {
                Console.SetCursorPosition(4, ALTO + 1);
                Console.Write("fil:{0} col:{1}", nave.fil, nave.col);
                Console.Write(" enem:{0}", enemigos.num);
                Console.Write(" balas:{0}", balas.num);
            }
        }
        static void InicializaEntidades(out Entidad nave, out GrEntidades enemigos, out GrEntidades balas, out GrEntidades colisiones)
        {
            nave = new Entidad();//Nave
            nave.col = ANCHO / 2;
            nave.fil = ALTO / 2;
            enemigos = new GrEntidades();//Enemigos
            enemigos.ent = new Entidad[MAX_ENEMIGOS];
            enemigos.num = 0;
            balas = new GrEntidades();//balas
            balas.ent = new Entidad[MAX_BALAS + 1];
            balas.num = 0;
            colisiones = new GrEntidades();//Colisiones
            colisiones.ent = new Entidad[MAX_ENEMIGOS + MAX_BALAS];
            colisiones.num = 0;
        }


        static void IniciaTunel(out Tunel tunel)
        {
            // creamos arrays
            tunel.suelo = new int[ANCHO];
            tunel.techo = new int[ANCHO];

            // rellenamos posicion 0 como semilla para generar el resto
            tunel.techo[0] = 0;
            tunel.suelo[0] = ALTO - 1;

            // dejamos 0 como la última y avanzamos hasta dar la vuelta
            tunel.ini = 1;
            for (int i = 1; i < ANCHO; i++)
            {
                AvanzaTunel(ref tunel);
            }
            // al dar la vuelta y quedará tunel.ini=0    
        }



        static void AvanzaTunel(ref Tunel tunel)
        {
            // ultima pos del tunel: anterior a ini de manera circular
            int ult = (tunel.ini + ANCHO - 1) % ANCHO;

            // valores de suelo y techo en la última posicion
            int s = tunel.suelo[ult],
                t = tunel.techo[ult]; // incremento/decremento de suelo/techo

            // generamos nueva columna a partir de esta última
            int opt = rnd.Next(5); // obtenemos un entero de [0,4]
            if (opt == 0 && s < ALTO - 1) { s++; t++; }   // tunel baja y mantiene ancho
            else if (opt == 1 && t > 0) { s--; t--; }   // sube y mantiene ancho
            else if (opt == 2 && s - t > 7) { s--; t++; } // se estrecha (como mucho a 5)
            else if (opt == 3)
            {                    // se ensancha, si puede
                if (s < ALTO - 1) s++;
                if (t > 0) t--;
            } // con 4 sigue igual

            // guardamos nueva columna del tunel generada
            tunel.suelo[tunel.ini] = s;
            tunel.techo[tunel.ini] = t;

            // avanzamos la tunel.ini: siguiente en el array circular
            tunel.ini = (tunel.ini + 1) % ANCHO;
        }



        static void EliminaEntidad(int i, ref GrEntidades gr)
        {
            gr.ent[i].col = gr.ent[gr.num - 1].col;
            gr.ent[i].fil = gr.ent[gr.num - 1].fil;
            gr.num--;
        }


        static char LeeInput()
        {
            char ch = ' ';
            if (Console.KeyAvailable)
            {
                string dir = Console.ReadKey(true).Key.ToString();
                if (dir == "A" || dir == "LeftArrow") ch = 'l';
                else if (dir == "D" || dir == "RightArrow") ch = 'r';
                else if (dir == "W" || dir == "UpArrow") ch = 'u';
                else if (dir == "S" || dir == "DownArrow") ch = 'd';
                else if (dir == "X" || dir == "Spacebar") ch = 'x'; // bala        
                else if (dir == "P") ch = 'p'; // pausa					
                else if (dir == "Q" || dir == "Escape") ch = 'q'; // salir
                while (Console.KeyAvailable) Console.ReadKey();
            }
            return ch;
        }

    }
}