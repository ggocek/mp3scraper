// Copyright © 2017, Gary Gocek, www.gocek.org
// See app.config for usage instructions.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.ServiceModel.Syndication;
using System.Reflection;

namespace mp3scraper
{
    /// <summary>
    /// Get HTML markup from feed, scrape it into dataset, search for MP3 links, write RSS podcast file.
    /// </summary>
    public class Program
    {
        public static mp3scrApi.Mp3scrApi mp3scr = new mp3scrApi.Mp3scrApi();

        public static void Main(string[] args)
        {
            mp3scr.ScrapeExtensionFromObj(".mp3"); // This program looks for MP3 links

            object o = null;
            SyndicationFeed sf = null;
            SyndicationLink mpSl = null;
            DateTimeOffset latestItemLastModDt = new DateTime(1900, 1, 1);
            string testIndexVal = string.Empty;

            try
            {
                // Get some filtering guidance from the config file
                o = ConfigurationManager.AppSettings["testIndex"];
                testIndexVal = (o == null) ? string.Empty : o.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main (filtering guidance): " + ex.Message);
                Environment.Exit(-2);
            }

            try
            {
                // The ServicePoint class provides connection management for HTTP connections.
                // This resolve occasional issues with some SSL sites
                // https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 9999;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main (ServicePoint): " + ex.Message);
                Environment.Exit(-3);
            }

            try
            {
                // Stuff for RSS generator property
                string verVal = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string apiVal = mp3scr.Ver;
                string genVal = "mp3scraper v" + verVal + ", mp3scrApi v" + apiVal + ", by Gary Gocek, http://www.gocek.org/software/";
                Console.WriteLine(genVal);

                for (int iii = 1; iii <= 9999; iii++)
                {

                    // If the test index is set, only process the matching index
                    if (!string.IsNullOrEmpty(testIndexVal) && (testIndexVal != iii.ToString("d4")))
                    {
                        Console.WriteLine(iii.ToString("d4"));
                        Console.WriteLine("Skipping feed " + iii.ToString("d4") + " because testIndex is set to " + testIndexVal);
                        continue;
                    }

                    o = ConfigurationManager.AppSettings["channelTitle" + iii.ToString("d4")];
                    mp3scr.ChannelTitleFromObj(o);
                    o = ConfigurationManager.AppSettings["enabled" + iii.ToString("d4")];
                    mp3scr.EnabledFromObj(o);
                    if (!mp3scr.Enabled)
                    {
                        // If the channel title is empty, then there is no configuration for the index.
                        if (!string.IsNullOrEmpty(mp3scr.ChannelTitle))
                        {
                            Console.WriteLine(iii.ToString("d4"));
                            Console.WriteLine("Skipping disabled feed " + mp3scr.ChannelTitle);
                        }
                        continue;
                    }

                    o = ConfigurationManager.AppSettings["url" + iii.ToString("d4")];
                    mp3scr.UrlFromObj(o);
                    if (string.IsNullOrEmpty(mp3scr.Url))
                        continue;
                    // Get the set of chars to look backward for, before the .mp3, usually just "=" but not always
                    o = ConfigurationManager.AppSettings["urlPrefix" + iii.ToString("d4")];
                    mp3scr.UrlPrefixFromObj(o);
                    // Get some filtering guidance from the config file
                    o = ConfigurationManager.AppSettings["mp3Filter" + iii.ToString("d4")];
                    mp3scr.Mp3FilterFromObj(o);
                    o = ConfigurationManager.AppSettings["indirectFilter" + iii.ToString("d4")];
                    mp3scr.IndirectFilterFromObj(o);
                    // Determine if the current file to be scraped contains MP3 links (direct)
                    // or links to files containing MP3 links.
                    o = ConfigurationManager.AppSettings["indirect" + iii.ToString("d4")];
                    mp3scr.IndirectFromObj(o);
                    // The config file specifies the expected date order of the URLs.
                    o = ConfigurationManager.AppSettings["sortDescending" + iii.ToString("d4")];
                    mp3scr.SortDescendingFromObj(o);
                    // Feed file naming
                    o = ConfigurationManager.AppSettings["existingFeedFolder" + iii.ToString("d4")];
                    mp3scr.ExistingFeedFolderFromObj(o);
                    o = ConfigurationManager.AppSettings["destBase" + iii.ToString("d4")];
                    mp3scr.DestBaseFromObj(o);
                    o = ConfigurationManager.AppSettings["destFolderName" + iii.ToString("d4")];
                    mp3scr.DestFolderNameFromObj(o);
                    // Channel and item value helpers
                    o = ConfigurationManager.AppSettings["guidPrefix" + iii.ToString("d4")];
                    mp3scr.GuidPrefixFromObj(o);
                    // The config file specifies whether an MP3 link should be retained in an existing feed
                    // if the scraped web page no longer contains the link.
                    o = ConfigurationManager.AppSettings["retainOrphans" + iii.ToString("d4")];
                    mp3scr.RetainOrphansFromObj(o);
                    // For use when prepending a relative name
                    o = ConfigurationManager.AppSettings["stripBaseName" + iii.ToString("d4")];
                    mp3scr.StripBaseNameFromObj(o);
                    // Prepend relative URLs with this
                    o = ConfigurationManager.AppSettings["prependRelative" + iii.ToString("d4")];
                    mp3scr.PrependRelativeFromObj(o);
                    // Number of days to wait to reprocess a feed
                    o = ConfigurationManager.AppSettings["refreshDays" + iii.ToString("d4")];
                    mp3scr.RefreshDaysFromObj(o);
                    o = ConfigurationManager.AppSettings["ftpPath" + iii.ToString("d4")];
                    mp3scr.FtpPathFromObj(o);
                    o = ConfigurationManager.AppSettings["ftpUserName" + iii.ToString("d4")];
                    mp3scr.FtpUserNameFromObj(o);
                    o = ConfigurationManager.AppSettings["ftpPassword" + iii.ToString("d4")];
                    mp3scr.FtpPasswordFromObj(o);
                    o = ConfigurationManager.AppSettings["permissionRevoked" + iii.ToString("d4")];
                    mp3scr.PermissionRevokedFromObj(o);
                    o = ConfigurationManager.AppSettings["channelNotes" + iii.ToString("d4")];
                    mp3scr.ChannelNotesFromObj(o);
                    // The config file specifies whether MP3 links should be forced to HTTP.
                    o = ConfigurationManager.AppSettings["allowHttps" + iii.ToString("d4")];
                    mp3scr.AllowHttpsFromObj(o);
                    // The config file specifies whether a formula should be used to wrap around to the beginning of a non-changing source feed
                    o = ConfigurationManager.AppSettings["wraparound" + iii.ToString("d4")];
                    mp3scr.WraparoundFromObj(o);

                    // Get any existing RSS file, for merging with the latest MP3s and checking the refresh date.
                    Console.WriteLine(iii.ToString("d4"));
                    Console.WriteLine("Looking for RSS for " + mp3scr.ChannelTitle + ": " + mp3scr.Url);
                    sf = null;
                    if (!string.IsNullOrEmpty(mp3scr.ExistingFeedFolder) && !string.IsNullOrEmpty(mp3scr.DestBase))
                    {
                        string existingRssUrl = System.IO.Path.Combine(mp3scr.ExistingFeedFolder, mp3scr.DestBase);
                        try
                        {
                            sf = mp3scr.RssFromUrl(existingRssUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            // Ignore exceptions from RssFromUrl - the remote RSS probably doesn't exist
                        }
                    }
                    // If testIndex is set, ignore refreshDays.
                    // If refreshDays is not set, ignore refreshDays.
                    // If there is no existing feed, ignore refreshDays.
                    if (string.IsNullOrEmpty(testIndexVal) && (sf != null) && (mp3scr.RefreshDays > 0))
                    {
                        // There is an existing feed and the number of days to refresh has been set.
                        // Determine the most recent item in the feed.
                        if (sf.Items.Count() > 0)
                        {
                            latestItemLastModDt = (from feedItem in sf.Items
                                                   select feedItem.LastUpdatedTime).Max();
                            // Determine if enough time has passed to refresh the feed.
                            if (DateTime.UtcNow < latestItemLastModDt.Add(new TimeSpan(mp3scr.RefreshDays, 0, 0, 0)))
                            {
                                // The current datetime is NOT refreshDays after the datetime of the most recent
                                // existing feed item. It's too soon to refresh the feed.
                                Console.WriteLine("Not yet time to refresh " + mp3scr.ChannelTitle + ": " + mp3scr.Url);
                                continue;
                            }
                        }
                    }

                    // Get the full markup of the web page
                    Console.WriteLine("Scraping " + mp3scr.Url);
                    string curMarkup = string.Empty;
                    try
                    {
                        curMarkup = mp3scr.Mp3PageMarkup(mp3scr.Url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }

                    List<string> curAddrs = new List<string>();
                    // Read the current file and get the links in that file.
                    try
                    {
                        if (!mp3scr.Indirect)
                        {
                            // These addresses will contain MP3 links
                            curAddrs = mp3scr.Mp3PageScrape(curMarkup, mp3scr.Mp3Filter, mp3scr.PrependRelative, mp3scr.StripBaseName, mp3scr.UrlPrefix);
                        }
                        else
                        {
                            // Each of these addresses will contain links to files containing links to MP3 files.
                            List<string> iAddrs = mp3scr.IndirectPageScrape(curMarkup, mp3scr.IndirectFilter);
                            foreach (string iMarkup in iAddrs)
                            {
                                // These addresses will contain MP3 links
                                curAddrs = mp3scr.Mp3PageScrape(curMarkup, mp3scr.Mp3Filter, mp3scr.PrependRelative, mp3scr.StripBaseName, mp3scr.UrlPrefix);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }

                    // Remove the duplicates from the list of addresses
                    List<string> noDupsCurAddrs = curAddrs.Distinct().ToList();
                    // Ensure that the addresses are sorted in reverse chronological order.
                    // If SortDescending is false, reverse the list now.
                    if (!mp3scr.SortDescending)
                        noDupsCurAddrs.Reverse();

                    // RSS items from an existing feed to retain
                    List<SyndicationItem> itemsToKeep = new List<SyndicationItem>();
                    // RSS items created from scraped MP3 links
                    List<SyndicationItem> itemsToAdd = new List<SyndicationItem>();

                    // In general, the scraped MP3 links are used to create new RSS items with new
                    // sizes and mod dates. Theoretically, an MP3 link could be saved in an RSS file
                    // but later removed from the host web page, but the file might still exist on
                    // the back end. If RetainOrphans is true, add RSS items to itemsToKeep if
                    // the RSS item's link is not found in noDupsCurAddrs.
                    // The host removed the MP3 link for a reason, so this could be considered discourteous.
                    // The links are tested for existence.
                    // If RetainOrphans is false, the RSS items are not retained and the links will be lost.
                    if ((sf != null) && mp3scr.RetainOrphans)
                    {
                        foreach (SyndicationItem si in sf.Items)
                        {
                            // Get the MP3 link.
                            // There are usually two link objects, one for the item's parent feed, and one for the MP3 file.
                            // If there are multiple MP3 links in one item, this will not work right.
                            mpSl = (from sil in si.Links
                                    where sil.Uri.AbsoluteUri.EndsWith(mp3scr.ScrapeExtension, StringComparison.InvariantCultureIgnoreCase)
                                    select sil).FirstOrDefault();
                            // If the RSS item's MP3 link is not in the list of scraped links, create an RSS item.
                            // The file will either be found with its current size and mod date,
                            // or the file will no longer exist and the RSS item will not be created.
                            if (!noDupsCurAddrs.Contains(mpSl.Uri.AbsoluteUri, StringComparer.InvariantCultureIgnoreCase))
                            {
                                Console.WriteLine("RSS item: " + mpSl.Uri.AbsoluteUri);
                                SyndicationItem newSi =
                                    mp3scr.ItemForMp3(mpSl.Uri.AbsoluteUri, mp3scr.ChannelTitle, mp3scr.GuidPrefix, mp3scr.Url, mp3scr.AllowHttps);
                                if (newSi != null)
                                {
                                    itemsToKeep.Add(newSi);
                                }
                                else
                                {
                                    Console.WriteLine("While retaining orphans, ItemForMp3 returned null for " + mpSl.Uri.AbsoluteUri);
                                }
                            }
                        }
                    }

                    // Create a new RSS item for each scraped MP3 link.
                    // itemsToKeep will not contain any of these (because they were tested above).
                    // Previously generated RSS link items are discarded and recreated.
                    foreach (string mp3Addr in noDupsCurAddrs)
                    {
                        Console.WriteLine("RSS item: " + mp3Addr);
                        SyndicationItem newSi = mp3scr.ItemForMp3(mp3Addr, mp3scr.ChannelTitle, mp3scr.GuidPrefix, mp3scr.Url, mp3scr.AllowHttps);
                        if (newSi != null)
                        {
                            itemsToAdd.Add(newSi);
                        }
                        else
                        {
                            Console.WriteLine("While creating RSS items, ItemForMp3 returned null for " + mp3Addr);
                        }
                    }

                    Console.WriteLine("Creating RSS feed...");
                    // Create the new RSS feed and add all the items
                    try
                    {
                        sf = mp3scr.GenerateFeed(mp3scr.ChannelTitle, mp3scr.Url, "en", mp3scr.PermissionRevoked, itemsToAdd, itemsToKeep, genVal, mp3scr.ChannelNotes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }

                    // Wraparound processing
                    if (mp3scr.Wraparound)
                    {
                        List<SyndicationItem> myItems = mp3scr.WrapAroundItems(sf);
                        if (myItems != null)
                        {
                            sf.Items = myItems;
                        }
                    }

                    // Save the feed
                    string localName = System.IO.Path.Combine(mp3scr.DestFolderName, mp3scr.DestBase);
                    Console.WriteLine("Saving " + localName);
                    sf.LastUpdatedTime = new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0));
                    using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(localName))
                    {
                        sf.SaveAsRss20(xmlWriter);
                    }

                    // Upload the feed file to the FTP folder
                    try
                    {
                        if (!string.IsNullOrEmpty(mp3scr.FtpPath))
                        {
                            mp3scr.FtpUpload(localName, mp3scr.DestBase, mp3scr.FtpPath, mp3scr.FtpUserName, mp3scr.FtpPassword);
                            Console.WriteLine("FtpUpload: " + mp3scr.FtpPath + mp3scr.DestBase + " uploaded successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }

                Console.WriteLine("mp3scraper done");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main: " + ex.Message);
                Environment.Exit(-1);
            }
        }
    }
}
