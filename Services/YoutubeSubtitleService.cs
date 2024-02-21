﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using YoutubeExplode;
using YoutubeExplode.Videos.ClosedCaptions;

namespace ReadVideo.Services.YoutubeManagement
{
    public class YoutubeSubtitleService : IYoutubeSubtitleService
    {
        public async Task<string> ExtractSubtitle(string videoId, string language, bool returnFullData)
        {
            try
            {

                // Create a new instance of YoutubeClient
                var youtube = new YoutubeClient();

                // Get the available subtitle tracks
                var tracks = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

                if (String.IsNullOrEmpty(language)) language = tracks.Tracks[0].Language.Code;

                // Select a track
                var trackInfo = tracks.GetByLanguage(language);//;?.WithAutoGenerated(false);

                if (trackInfo != null)
                {
                    // Get the actual subtitle track
                    var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

                    if (returnFullData)
                    {
                        // Assuming 'track.Captions' is a collection of some type that has 'Text' and 'Offset' properties
                        var fullChunks = track.Captions
                            .Where(caption => !string.IsNullOrEmpty(caption.Text.Trim()))
                            .Select(caption => caption)//new { Text = caption.Text, Offset = caption.Offset, Parts = caption.Parts,Duration = caption.Duration }) // Using anonymous type here
                            .ToList();

                        // Convert the list of anonymous types to a JSON string
                        return JsonSerializer.Serialize(fullChunks, new JsonSerializerOptions
                        {
                            WriteIndented = true // Optional: makes the JSON string more readable
                        });
                    }

                    List<SubtitleChunk> chunks = track.Captions.Where(caption => !String.IsNullOrEmpty(caption.Text.Trim())).Select(caption => new SubtitleChunk { Text = caption.Text, Offset = caption.Offset }).ToList();

                    // Convert the list of StructureB to a JSON string
                    return JsonSerializer.Serialize(chunks, new JsonSerializerOptions
                    {
                        WriteIndented = true // Optional: makes the JSON string more readable
                    });

                }

                return "";
            }
            catch (Exception)
            {

                throw;
            }

        }

    
        public async Task<string> ExtractSubtitleAsTextBlocks(string videoId, string language, int sentenceMinTimeSpan)
        {
            try
            {

                // Create a new instance of YoutubeClient
                var youtube = new YoutubeClient();

                var videoInfo = await youtube.Videos.GetAsync(videoId);
                // Get the available subtitle tracks
                var tracks = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

                if (String.IsNullOrEmpty(language)) language = tracks.Tracks[0].Language.Code;

                // Select a track
                var trackInfo = tracks.GetByLanguage(language);//;?.WithAutoGenerated(false);

                if (trackInfo != null)
                {
                    // Get the actual subtitle track
                    var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

                    var fullChunks = track.Captions.ToList();
                        //.Where(caption => !string.IsNullOrEmpty(caption.Text.Trim()))
                        //.ToList();

                    List<ClosedCaptionPart> rawParts = new List<ClosedCaptionPart>();
                    for (int i = 0; i < fullChunks.Count; i++)
                    {

                        for (int j = 0; j < fullChunks[i].Parts.Count; j++)
                        {
                            rawParts.Add(new ClosedCaptionPart(fullChunks[i].Parts[j].Text,
                                fullChunks[i].Offset + fullChunks[i].Parts[j].Offset)
                            );

                        }

                    }

                    var textChunkLists = new List<List<ClosedCaptionPart>>();
                    List<ClosedCaptionPart> blockBuilder = new List<ClosedCaptionPart>();

                    //TODO if (rawParts.Count == 0) throw;

                    blockBuilder.Add(rawParts[0]);

                    for (int i = 1; i < rawParts.Count; i++)
                    {
                        double msDifferenceFromPreviousBlock = (rawParts[i].Offset - rawParts[i-1].Offset).TotalMilliseconds;
                        
                        if (msDifferenceFromPreviousBlock > sentenceMinTimeSpan && blockBuilder.Count>0)
                        {
                            textChunkLists.Add(blockBuilder);
                            blockBuilder = new List<ClosedCaptionPart>();
                        }

                        blockBuilder.Add(rawParts[i]);
                    }

                    // MergeBlocks when previous having small element amount
                    // assuming it is the same info block

                    for (int i = 1; i < textChunkLists.Count; i++)
                    {
                        if (textChunkLists[i - 1].Count <= 25)
                        {
                            textChunkLists[i - 1].AddRange(textChunkLists[i]);
                            textChunkLists.RemoveAt(i);

                            i--;
                        }
                    }


                    // TODO: remove textChunkLists with 0 elements

                    // Convert the list of anonymous types to a JSON string
                    return JsonSerializer.Serialize(
                        new { 
                            VideoInfo=videoInfo,
                            Captions = textChunkLists.Select
                                (p => new { Offset = p[0].Offset, 
                                    Text = ContentPreprocess(String.Join(" ", p.Select(obj => obj.Text))) }
                                )  
                        }

                        , new JsonSerializerOptions
                    {
                        WriteIndented = true // Optional: makes the JSON string more readable
                    });


                }

                return "";
            }
            catch (Exception)
            {

                throw;
            }

        }

        public Func<string, string> ContentPreprocess = text => text.Trim();//.Replace("'", "APO")


        //public int CountWords(string sentence)
        //{
        //    if (string.IsNullOrWhiteSpace(sentence))
        //    {
        //        return 0; // Returns 0 if the sentence is null, empty, or consists only of white-space characters.
        //    }

        //    // Split the sentence by spaces and count the number of elements in the resulting array.
        //    // This uses the StringSplitOptions.RemoveEmptyEntries option to exclude any empty entries
        //    // that can occur if there are multiple spaces between words.
        //    return sentence.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        //}

    }

    internal class SubtitleChunk
    {
        public string Text { get; set; }
        public TimeSpan Offset { get; set; }
    }
}
