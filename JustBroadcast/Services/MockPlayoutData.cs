using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    // Dev-only sample data used when UseMockPlayoutData is true (API/hub offline).
    public static class MockPlayoutData
    {
        // 16:9 SMPTE-style colour-bar test pattern (JPEG) shown as the mock preview frame.
        public const string SampleFrameBase64 =
            "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAA0JCgsKCA0LCgsODg0PEyAVExISEyccHhcgLikxMC4pLSwzOko+MzZGNywtQFdBRkxO" +
            "UlNSMj5aYVpQYEpRUk//2wBDAQ4ODhMREyYVFSZPNS01T09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09P" +
            "T09PT09PT0//wAARCAC0AUADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUF" +
            "BAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVW" +
            "V1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi" +
            "4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAEC" +
            "AxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVm" +
            "Z2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq" +
            "8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDp6KKKACiiigAqK4/1Y+tS1Fcf6sfWuXHf7vP0KjuV6KKK+SNwooooAKo3v+uH+7V6qN7/" +
            "AK4f7tevkn+9r0Z5ecf7s/VFeiiivsj5MKKKKACqb/6xvqauVTf/AFjfU14+cfBH1NKe4lFFFeAahRRRQBFdf8e7fh/Os+tC6/49" +
            "2/D+dZ9fXZB/u0v8T/JH3PDP+6S/xP8AJBRRRXuH0YUUUUAQXP8AD+NQVPc/w/jUFfK5l/vMvl+SN4fCFFFFcRQUUUUAZ91/x8N+" +
            "H8qiqW6/4+G/D+VRV+gYL/dqf+FfkfD4v/eJ+r/MKKKK6TnCiiigD2iiiiviDpCiiigAqK4/1Y+tS1Fcf6sfWuXHf7vP0KjuV6KK" +
            "K+SNwooooAKo3v8Arh/u1eqje/64f7tevkn+9r0Z5ecf7s/VFeiiivsj5MKKKKACqb/6xvqauVTf/WN9TXj5x8EfU0p7iUUUV4Bq" +
            "FFFFAEV1/wAe7fh/Os+tC6/492/D+dZ9fXZB/u0v8T/JH3PDP+6S/wAT/JBRRRXuH0YUUUUAQXP8P41BU9z/AA/jUFfK5l/vMvl+" +
            "SN4fCFFFFcRQUUUUAZ91/wAfDfh/Koqluv8Aj4b8P5VFX6Bgv92p/wCFfkfD4v8A3ifq/wAwooorpOcKKKKAPaKKKK+IOkKKKKAC" +
            "orj/AFY+tS1Fcf6sfWuXHf7vP0KjuV6KKK+SNwooooAKo3v+uH+7V6qN7/rh/u16+Sf72vRnl5x/uz9UV6KKK+yPkwooooAKpv8A" +
            "6xvqauVTf/WN9TXj5x8EfU0p7iUUUV4BqFFFFAEV1/x7t+H86z60Lr/j3b8P51n19dkH+7S/xP8AJH3PDP8Aukv8T/JBRRRXuH0Y" +
            "UUUUAQXP8P41BU9z/D+NQV8rmX+8y+X5I3h8IUUUVxFBRRRQBn3X/Hw34fyqKpbr/j4b8P5VFX6Bgv8Adqf+FfkfD4v/AHifq/zC" +
            "iiiuk5wooooA9oooor4g6QooooAKiuP9WPrUtRXH+rH1rlx3+7z9Co7leiiivkjcKKKKACqN7/rh/u1eqje/64f7tevkn+9r0Z5e" +
            "cf7s/VFeiiivsj5MKKKKACqb/wCsb6mrlU3/ANY31NePnHwR9TSnuJRRRXgGoUUUUARXX/Hu34fzrPrQuv8Aj3b8P51n19dkH+7S" +
            "/wAT/JH3PDP+6S/xP8kFFFFe4fRhRRRQBBc/w/jUFT3P8P41BXyuZf7zL5fkjeHwhRRRXEUFFFFAGfdf8fDfh/Koqluv+Phvw/lU" +
            "VfoGC/3an/hX5Hw+L/3ifq/zCiiiuk5wooooA9oooor4g6QooooAKiuP9WPrUtRXH+rH1rlx3+7z9Co7leiiivkjcKKKKACqN7/r" +
            "h/u1eqje/wCuH+7Xr5J/va9GeXnH+7P1RXooor7I+TCiiigAqm/+sb6mrlU3/wBY31NePnHwR9TSnuJRRRXgGoUUUUARXX/Hu34f" +
            "zrPrQuv+Pdvw/nWfX12Qf7tL/E/yR9zwz/ukv8T/ACQUUUV7h9GFFFFAEFz/AA/jUFT3P8P41BXyuZf7zL5fkjeHwhRRRXEUFFFF" +
            "AGfdf8fDfh/Koqluv+Phvw/lUVfoGC/3an/hX5Hw+L/3ifq/zCiiiuk5wooooA9oooor4g6QooooAKiuP9WPrUtRXH+rH1rlx3+7" +
            "z9Co7leiiivkjcKKKKACqN7/AK4f7tXqo3v+uH+7Xr5J/va9GeXnH+7P1RXooor7I+TCiiigAqm/+sb6mrlU3/1jfU14+cfBH1NK" +
            "e4lFFFeAahRRRQBFdf8AHu34fzrPrQuv+Pdvw/nWfX12Qf7tL/E/yR9zwz/ukv8AE/yQUUUV7h9GFFFFAEFz/D+NQVPc/wAP41BX" +
            "yuZf7zL5fkjeHwhRRRXEUFFFFAGfdf8AHw34fyqKpbr/AI+G/D+VRV+gYL/dqf8AhX5Hw+L/AN4n6v8AMKKKK6TnCiiigD2iiiiv" +
            "iDpCiiigAqK4/wBWPrUtRXH+rH1rlx3+7z9Co7leiiivkjcKKKKACqN7/rh/u1eqje/64f7tevkn+9r0Z5ecf7s/VFeiiivsj5MK" +
            "KKKACqb/AOsb6mrlU3/1jfU14+cfBH1NKe4lFFFeAahRRRQBFdf8e7fh/Os+tC6/492/D+dZ9fXZB/u0v8T/ACR9zwz/ALpL/E/y" +
            "QUUUV7h9GFFFFAEFz/D+NQVPc/w/jUFfK5l/vMvl+SN4fCFFFFcRQUUUUAZ91/x8N+H8qiqW6/4+G/D+VRV+gYL/AHan/hX5Hw+L" +
            "/wB4n6v8wooorpOcKKKKAPT/APhK9D/5/v8AyE/+FH/CV6H/AM/3/kJ/8K8x3D1FG4eor4qx0np3/CV6H/z/AH/kJ/8ACj/hK9D/" +
            "AOf7/wAhP/hXmO4eoo3D1FFgPTv+Er0P/n+/8hP/AIVrzRuyAKMnPrXjW4eor17+2tJ/6Cll/wCBCf41lWpKpBwezBO2ofZ5f7v6" +
            "ij7PL/d/UUf21pP/AEFLL/wIT/Gj+2tJ/wCgpZf+BCf41539kUe7/D/Iv2jD7PL/AHf1FH2eX+7+oo/trSf+gpZf+BCf40f21pP/" +
            "AEFLL/wIT/Gj+yKPd/h/kHtGYv8Awkuj/wDP5/5Cf/Cql14g0qSQFLrIxj/Vt/hXDbh6ijcPUV04XA08NU9pBu/n/wAMYYmmsRD2" +
            "c9vI7P8AtzTf+fn/AMcb/Cj+3NN/5+f/ABxv8K4zcPUUbh6ivT9vI87+yKHd/h/kdn/bmm/8/P8A443+Fbf9nXf/ADy/8eH+NeY7" +
            "h6ivXv7a0n/oKWX/AIEJ/jQ68g/sih3f4f5Gf/Z13/zy/wDHh/jVZ9Jvi5Ig4J/vr/jWz/bWk/8AQUsv/AhP8aP7a0n/AKCll/4E" +
            "J/jXLiYfWElPp2KWVUV1f4f5GL/ZN/8A88P/AB9f8aP7Jv8A/nh/4+v+NbX9taT/ANBSy/8AAhP8aP7a0n/oKWX/AIEJ/jXH/Z9P" +
            "u/6+RX9mUe7/AA/yOK/tew/57/8Ajjf4Uf2vYf8APf8A8cb/AArltw9RRuHqKr+z6Xd/18hf2ZS7v8P8jpZ9UsnhZVnyT/sN/hVT" +
            "7dbf89P/AB01i7h6ijcPUV6OEm8JBwhte+p62BqvBU3Tp6pu+vy9Oxtfbrb/AJ6f+Omtr+wNU/59f/Ii/wCNcXuHqK9e/trSf+gp" +
            "Zf8AgQn+NdTx9Tsv6+Z2/wBp1ey/H/M5X+wNU/59f/Ii/wCNH9gap/z6/wDkRf8AGuq/trSf+gpZf+BCf40f21pP/QUsv/AhP8aX" +
            "1+r2X9fMP7Tq9l+P+ZyE/h3Vn27bTOP+mif41F/wjWsf8+f/AJFT/Gu0/trSf+gpZf8AgQn+NH9taT/0FLL/AMCE/wAa8+vH21R1" +
            "JbstZtWStZfj/meYfaIv7/6Gj7RF/f8A0NUNw9RRuHqKj6tEP7Xr9l+P+Zf+0Rf3/wBDR9oi/v8A6GqG4eoo3D1FH1aIf2vX7L8f" +
            "8x85DzMy8g1r/wDCI69/z4f+Rk/+KrF3D1Fevf21pP8A0FLL/wACE/xr16eZVqUIwilZK3Xp8zyqn7ybm927nnf/AAiOvf8APh/5" +
            "GT/4qj/hEde/58P/ACMn/wAVXon9taT/ANBSy/8AAhP8aP7a0n/oKWX/AIEJ/jV/2vX7L8f8yPZo87/4RHXv+fD/AMjJ/wDFUf8A" +
            "CI69/wA+H/kZP/iq9E/trSf+gpZf+BCf40f21pP/AEFLL/wIT/Gj+16/Zfj/AJh7NHjVFFFeaWFFFFABRRRQAUUUUAFFFFABRRRQ" +
            "AUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUU" +
            "UAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFaOnWkcpQTxklrqBME" +
            "kfI24n88Cs6rIvrlfKxJjyWVkO0dV6Z45x70wLq2kTXGnCSCNRPPscRS7lZcr33HB5Pf0pjwW5tJnMcSzpEW2xSFlHzoAc5PPLDG" +
            "fSqhvJ/NikDKrQtuTaiqAfXAGOw/KmQXEluXMZX512sGQMCMg9CPUCgRbsLNJ7SZnC72ysRLgHIGTgZ5/hHfrU1va2kkKTspIkQj" +
            "ywxyGRdzfmAP++/aqBu5vMjkDKrRNuTagUA9c4Ax2pq3EyCMI5URuXTHGGOMn9BQMvW0NvdwyS+QsZjDjarNhv3bsDyScgr+tFrD" +
            "B9p02OSBZFuQBJuZh1lZcjBHYCqhvbgsjB1XYSQERVHPXgDBpDdzm4jn3gSREFMKAFwcjA6UANOJ5gI444s9AGwv5sf602NhHJl4" +
            "0kA42sTj9CKZRSA2ZreASX4itLceRcCJA8rKNvz9yw5+UVXjhge0JSKN5QrM672DLgk5XnBXA579ari+uA0zFkYzPvffGrAtzzgj" +
            "jqenrSfbJ/KMe5duCPuDIB6gHGQOTwPWmIlvfJFtamO2jjaWMuzKWPO9h3J7AVdubK2+3NAI4Y9twVASXOYhksWyTggAenesh5Hd" +
            "Y1c5Ea7V46DJP8yac1xM00spf55s7zgc56/SgZoCCCO/kja2ieNoXmT52IXEZbAIbkAjH4VCrQNp8032OEOsqICGfgMHJ/i/2RVV" +
            "bmZdmH+4jRrwOFbOR/48fzpokcQtED8jMGIx1IyB/M0Aa8tpCNWltxbWwjQzbAJic7VYjd83HQelH2G2e3dTEqTyCNUKOSqu28jB" +
            "ycghVHU4JNZ0l/cSSNIzR723bmWJQTuBByQO4JphuZjAIS/yLjAwO27HP/Am/OgQ++iWG4VEXaPKjYj3KKT+pNVqlubiW6naad98" +
            "jYycAdBgdPYVFQMKKKKQBRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFA" +
            "BRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAf/9k=";

        public static List<PlayoutoutputShortDto> Outputs(string playoutId) => new()
        {
            new() { Id = $"{playoutId}-o1", PlayoutId = playoutId, OutputName = "SDI Main", OutputType = "1",
                    VideoFormatName = "1080i50", AudioFormatName = "PCM 2ch", HardwareOutput = "Decklink 1" },
            new() { Id = $"{playoutId}-o2", PlayoutId = playoutId, OutputName = "RTMP CDN", OutputType = "8",
                    VideoFormatName = "1080p50", AudioFormatName = "AAC 128k", Url = "rtmp://cdn.example/live" },
            new() { Id = $"{playoutId}-o3", PlayoutId = playoutId, OutputName = "DVB UDP", OutputType = "3",
                    VideoFormatName = "720p50", AudioFormatName = "MP2", Url = "239.0.0.1", Port = "1234" },
        };

        public static List<EventlogDto> EventLogs(string playoutId)
        {
            var now = DateTime.Now;
            return new()
            {
                new() { Id = "1", PlayoutId = playoutId, LogType = 1, Time = now.AddMinutes(-1), Description = "Output SDI Main started", ItemType = 3 },
                new() { Id = "2", PlayoutId = playoutId, LogType = 0, Time = now.AddMinutes(-4), Description = "Playlist loaded", ItemType = 2 },
                new() { Id = "3", PlayoutId = playoutId, LogType = 2, Time = now.AddMinutes(-9), Description = "Input jitter above threshold", ItemType = 1 },
                new() { Id = "4", PlayoutId = playoutId, LogType = 3, Time = now.AddMinutes(-15), Description = "RTMP reconnect", ItemType = 3 },
                new() { Id = "5", PlayoutId = playoutId, LogType = 0, Time = now.AddMinutes(-22), Description = "System startup", ItemType = 0 },
            };
        }

        public static List<ErrorListDto> Errors(string playoutName)
        {
            var now = DateTime.Now;
            var r = new Random(playoutName.GetHashCode());
            var list = new List<ErrorListDto>();
            string[] msgs = { "Buffer underrun", "Stream connection lost", "Audio format mismatch", "Output frames drop", "Network timeout" };
            for (int i = 0; i < 12; i++)
            {
                list.Add(new ErrorListDto
                {
                    Id = i.ToString(),
                    Type = r.Next(0, 3),
                    Time = now.AddDays(-r.Next(0, 7)).AddHours(-r.Next(0, 12)),
                    Description = msgs[r.Next(msgs.Length)],
                    PlayoutName = playoutName
                });
            }
            return list;
        }

        public static NextScheduleItemDto PushedList() => new()
        { Name = "Evening Block", ItemType = "4", TotalCount = 6, NextRun = DateTime.Now.AddMinutes(12).AddSeconds(34) };

        public static NextScheduleItemDto DailyList() => new()
        { Name = "Daily Rotation", ItemType = "1", TotalCount = 3, NextRun = DateTime.Now.AddMinutes(47).AddSeconds(8) };

        public static NextScheduleItemDto NextItem() => new()
        { Name = "Promo", ItemType = "2", TotalCount = 1, NextRun = DateTime.Now.AddMinutes(2).AddSeconds(19) };
    }
}
