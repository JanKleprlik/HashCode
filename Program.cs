﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace GoogleHascode
{
    class IntComparer : IComparer<int>
    {
        int IComparer<int>.Compare(int x, int y)
        {
            return -1 * x.CompareTo(y);
        }
    }

    class Book : IComparable<Book>
    {
        public int id;
        public int score;

        public int CompareTo(Book other)
        {
            return -1 * this.score.CompareTo(other.score);
        }
    }

    class Library
    {
        public int id;
        public List<int> books = new List<int>();
        public int num_of_books;
        public int num_of_used_books;
        public int scann_time;
		public int scans_per_day;
		public float score;        
	}


	class Program
	{
		public static void PrintOutput(List<Library> chosen_libraries)
		{
			using (var sw = new StreamWriter("output.txt"))
			{
				sw.WriteLine(chosen_libraries.Count);
				for (int i = 0; i < chosen_libraries.Count; i++)
				{
					sw.WriteLine("{0} {1}", chosen_libraries[i].id, chosen_libraries[i].num_of_books);

                    int j = 0;
                    for (; j < chosen_libraries[i].num_of_books - 1; j++)
                    {
                        sw.Write("{0} ", chosen_libraries[i].books[j]);
                    }
                    sw.WriteLine("{0}", chosen_libraries[i].books[j]);
                }
            }
        }

        public static int ReadInteger(ref StreamReader sr)
        {
            int number = 0;
            int digit = sr.Read();
            while ((digit >= 48) && (digit <= 57))
            {
                number = number * 10 + digit - 48;
                digit = sr.Read();
            }
            return (number);
        }


        static void Main(string[] args)
        {


            int number_of_books;
            int number_of_libraries;
            int number_of_days;
            float max_lib_score = 0;
            List<int> books = new List<int>();
            List<Library> libraries = new List<Library>();

            #region READING INPUT
            StreamReader sr = new StreamReader("a_example.txt");
            //StreamReader sr = new StreamReader("b_read_on.txt");
            //StreamReader sr = new StreamReader("c_incunabula.txt");
            //StreamReader sr = new StreamReader("d_tough_choices.txt");
            //StreamReader sr = new StreamReader("e_so_many_books.txt");
            //StreamReader sr = new StreamReader("f_libraries_of_the_world.txt");

            //reading first line
            string line = sr.ReadLine();
            string[] nums_s = line.Split(' ');

            number_of_books = int.Parse(nums_s[0]);
            number_of_libraries = int.Parse(nums_s[1]);
            number_of_days = int.Parse(nums_s[2]);

            //reading second line
            for (int i = 0; i < number_of_books; i++)
            {
                //ReadInteger(sr);

                books.Add(ReadInteger(ref sr));

            }


            //reading two lines for libraries
            {
                long score = 0;
                for (int i = 0; i < number_of_libraries; i++)
                {
                    Library lib = new Library();
                    lib.id = i;
                    line = sr.ReadLine();
                    nums_s = line.Split(' ');

                    lib.num_of_books = int.Parse(nums_s[0]);
                    lib.scann_time = int.Parse(nums_s[1]);
                    lib.scans_per_day = int.Parse(nums_s[2]);


                    if (!(lib.scann_time > number_of_days)) //doba nahrání knihovny je delší jak počet možných dnů
                    {
                        //přidám id knížek do knihovny
                        for (int j = 0; j < lib.num_of_books; j++)
                        {
                            int id = ReadInteger(ref sr);
                            score += books[id];
                            lib.books.Add(id);
                        }
                        lib.score = (score / (float)lib.scann_time);

						if (lib.score > max_lib_score)
						{
							max_lib_score = lib.score;
						}
					}
					libraries.Add(lib);
				}
			}
			#endregion





			#region Algorithm
			long ScoreOpt = Math.Min(1000000, (long)max_lib_score);
			long maxScore = (long)max_lib_score;
            long LibCoutOpt = libraries.Count;

			ScaleScore(libraries, ScoreOpt / maxScore);


			Cell[,] bag = new Cell[ScoreOpt, LibCoutOpt];

            for (int i = 0; i < ScoreOpt; i++) // Init level 0
                bag[i, 0] = new Cell(0, 0, 0, false);

            // Calculate other rows
            for (int score = 1; score < ScoreOpt; score++)
            {
                for (int lib = 0; lib < LibCoutOpt; lib++)
                {
                    CalculateCell(bag, libraries, score, lib);
                }
            }

			//Get Best Result
			long best_row = 0;
            for (long i = ScoreOpt - 1; i > 0; i--)
			{
				if (bag[i, libraries.Count-1].TotalTime <= number_of_days)
				{
					best_row = i;
					break;
				}
				//int a = bag[ScoreOpt - 1, i].TotalTime;
			}

            //Backtrack and recieve libraries
            //TODO CHANGE BACKTRACK PARAMETERS!!!!!!
			//ints should be longs
            int Y = (int)libraries.Count - 1;
            int X = (int)best_row;
            List<Library> used_libraries = new List<Library>();
			used_libraries = BackTrack(bag, X, Y, used_libraries, libraries);
			#endregion
			//for (; ; );
			//FindBestLibraryScore(libraries[0]);

            PrintOutput(used_libraries);
        }

		#region Algorithm

		public static void ScaleScore(List<Library> libraries, long constant)
		{
			foreach (var lib in libraries)
			{
				lib.score = lib.score * constant;
			}
		}

        /*
        static void FindBestLibraryScore (Library lib)
        {
            List<Book> books_in_lib = new List<Book>();
            for (int i = 0; i < lib.books.Count; i++)
            {
                books_in_lib.Add(new Book { id = i, score = books[i] });
            }
            IntComparer ic = new IntComparer();
            books_in_lib.Sort();
            books.Sort(ic);

            int score = 0;
            foreach (int i in books)
                score += i;

            float best = score / (float)lib.scann_time;
            float newscore = 0;
            int books_removed = 0;
            int best_books_removed = 0;
            for (int i = books_in_lib.Count - 1; i >= 0; i--)
            {
                score -= books[i];
                books_removed++;
                newscore = score / (float)lib.scann_time;
                if (newscore >= best)
                {
                    best = newscore;
                    books_removed = best_books_removed;
                }

        }*/

        static List<Library> BackTrack(Cell[,] bag, int X, int Y, List<Library> used_libraries, List<Library> libraries)
        {
            if (X == 0 || Y == 0)
                return used_libraries;

            if (bag[X,Y].IsLibraryUsed)
                used_libraries.Add(libraries[Y]);

            return BackTrack(bag, bag[X, Y].PrevCellX, bag[X, Y].PrevCellY, used_libraries, libraries);
        }

        static void CalculateCell(Cell[,] bag, List<Library> libraries, int X, int Y)
        {
			if (Y == 0)
				Y = 1;
			int indexY = (int)libraries[Y - 1].score;
			if (indexY > Y)
				indexY = 0;
			// X is Libraries Y is score
			if (bag[X - 1, Y].TotalTime < bag[X - 1, Y - indexY].TotalTime)
            {
                bag[X, Y] = new Cell(X - 1, Y, bag[X - 1, Y].TotalTime, false);
            }
            else
            {
                bag[X, Y] = new Cell(X - 1, Y - indexY, bag[X - 1, Y - indexY].TotalTime + (int)libraries[Y - 1].scann_time, true);
            }
        }


        struct Cell
        {
            public bool IsLibraryUsed;

            public int PrevCellX { get; }

            public int PrevCellY { get; }

            public int TotalTime { get; }

            public Cell(int PrevCellX, int PrevCellY, int TotalTime, bool used)
            {
                this.PrevCellX = PrevCellX;
                this.PrevCellY = PrevCellY;
                this.TotalTime = TotalTime;
                IsLibraryUsed = used;
            }

			public override string ToString()
			{
				return $"{TotalTime}";
			}
		}
		

        #endregion
    }
}
