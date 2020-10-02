using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog.Web;

namespace MediaLibrary
{
    public class MovieFile
    {
        // public property
        public string filePath { get; set; }
        public List<Media> Movies { get; set; }
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();

        // constructor is a special method that is invoked
        // when an instance of a class is created
        public MovieFile(string movieFilePath)
        {
            filePath = movieFilePath;
            Movies = new List<Media>();

            // to populate the list with data, read from the data file
            try
            {
                StreamReader sr = new StreamReader(filePath);
                // first line contains column headers
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    // create instance of Movie class
                    Movie movie = new Movie();
                    string line = sr.ReadLine();
                    // first look for quote(") in string
                    // this indicates a comma(,) in movie title
                    int qIdx = line.IndexOf('"');
                    int cIdx = line.IndexOf(',');
                    if (qIdx == -1)
                    {
                        // no quote = no comma in movie title
                        // movie details are separated with comma(,)
                        string[] movieDetails = line.Split(',');
                        movie.mediaId = UInt64.Parse(movieDetails[0]);
                        movie.title = movieDetails[1];
                        movie.director = movieDetails[2];
                        movie.runningTime = TimeSpan.Parse(movieDetails[3]);
                        movie.genres = movieDetails[4].Split('|').ToList();
                    }
                    else
                    {
                        // quote = comma in movie title
                        // extract the movieId
                        movie.mediaId = UInt64.Parse(line.Substring(0, cIdx));
                        // remove movieId and first quote from string
                        movie.director = line.Substring(1, cIdx);
                        movie.runningTime = TimeSpan.Parse(line.Substring(1, cIdx));
                        line = line.Substring(qIdx + 1);
                        // find the next quote
                        qIdx = line.IndexOf('"');
                        // extract the movieTitle
                        movie.title = line.Substring(0, qIdx);
                        // remove title and last comma from the string
                        line = line.Substring(qIdx + 2);
                        // replace the "|" with ", "
                        movie.genres = line.Split('|').ToList();
                    }
                    Movies.Add(movie);
                }
                // close file when done
                sr.Close();
                logger.Info("Movies in file {Count}", Movies.Count);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
         // public method
        public bool isUniqueTitle(string title)
        {
            if (Movies.ConvertAll(m => m.title.ToLower()).Contains(title.ToLower()))
            {
                logger.Info("Duplicate movie title {Title}", title);
                return false;
            }
            return true;
        }

        public void AddMovie(Movie movie)
        {
            try
            {
                // first generate movie id
                movie.mediaId = Movies.Max(m => m.mediaId) + 1;
                StreamWriter sw = new StreamWriter(filePath, true);
                sw.WriteLine($"{movie.mediaId},{movie.title},{movie.director},{movie.runningTime},{string.Join("|", movie.genres)}");
                sw.Close();
                // add movie details to Lists
                Movies.Add(movie);
                // log transaction
                logger.Info("Movie id {Id} added", movie.mediaId);
            } 
            catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }

}