using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.Benchmarks
{
    public class FileUploadBufferSize
    {
        //  Method | BufferSizeInBytes |    Median |   StdDev |
        // ------- |------------------ |---------- |--------- |
        //    Send |                16 | 31.7473 s | 0.2078 s |
        //    Send |                32 | 16.1486 s | 0.1826 s |
        //    Send |                64 |  8.2620 s | 0.2921 s |
        //    Send |               128 |  4.3023 s | 0.1761 s |
        //    Send |               256 |  2.4541 s | 0.0181 s |
        //    Send |               512 |  2.1071 s | 0.1032 s |
        //    Send |              1024 |  2.1312 s | 0.0202 s |
        //    Send |              4096 |  2.3398 s | 0.0777 s |
        //    Send |             16384 |  2.7483 s | 0.0920 s |

        //  Method | BufferSizeInBytes |   Median |   StdDev |
        // ------- |------------------ |--------- |--------- |
        //    Send |               256 | 2.4800 s | 0.0872 s |
        //    Send |               512 | 2.0880 s | 0.0124 s |
        //    Send |               768 | 2.1244 s | 0.0535 s |
        //    Send |              1024 | 2.1995 s | 0.0360 s |

        // Method | BufferSizeInBytes |   Median |   StdDev |
        //------- |------------------ |--------- |--------- |
        //   Send |               492 | 1.9850 s | 0.0407 s |
        //   Send |               496 | 1.9979 s | 0.0162 s |
        //   Send |               500 | 1.9697 s | 0.0518 s |
        //   Send |               504 | 1.8892 s | 0.0474 s |
        //   Send |               508 | 1.9317 s | 0.0723 s |
        //   Send |               512 | 1.9768 s | 0.0555 s |

        // Method | BufferSizeInBytes |   Median |   StdDev |
        //------- |------------------ |--------- |--------- |
        //   Send |               500 | 1.8099 s | 0.0435 s |
        //   Send |               501 | 1.8497 s | 0.0408 s |
        //   Send |               502 | 1.8091 s | 0.0610 s |
        //   Send |               503 | 1.8625 s | 0.0549 s |
        //   Send |               504 | 1.8777 s | 0.0419 s |
        //   Send |               505 | 1.9893 s | 0.0976 s |
        //   Send |               506 | 1.9048 s | 0.0657 s |
        //   Send |               507 | 1.8716 s | 0.0586 s |
        //   Send |               508 | 1.8423 s | 0.1377 s |

        static string filename;


        [Params(500, 501, 502, 503, 504, 505, 506, 507, 508)]
        public int BufferSizeInBytes
        {
            get;
            set;
        }

        static FileUploadBufferSize()
        {
            // Create a 15 MB file to push
            filename = "data.bin";

            // 1 KB buffer
            char[] buffer = new char[1024];

            using (StreamWriter writer = new StreamWriter(filename))
            {
                for (int i = 0; i < 15; i++)
                {
                    for (int j = 0; j < 1024; j++)
                    {
                        writer.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        [Benchmark]
        public void Send()
        {
            var device = AdbClient.Instance.GetDevices().Single();

            using (SyncService service = new SyncService(device))
            using (Stream stream = File.OpenRead(filename))
            {
                service.MaxBufferSize = this.BufferSizeInBytes;
                service.Push(stream, "/data/local/tmp/file.bin", 666, DateTime.Now, CancellationToken.None);
            }
        }
    }
}
