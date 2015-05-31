using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public static class ErrorCodeHelper {
		/** Result code for transfer success. */
    public const int RESULT_OK = 0;
    /** Result code for canceled transfer */
    public const int RESULT_CANCELED = 1;
    /** Result code for unknown error */
    public const int RESULT_UNKNOWN_ERROR = 2;
    /** Result code for network connection error */
    public const int RESULT_CONNECTION_ERROR = 3;
    /** Result code for unknown remote object during a pull */
    public const int RESULT_NO_REMOTE_OBJECT = 4;
    /** Result code when attempting to pull multiple files into a file */
    public const int RESULT_TARGET_IS_FILE = 5;
    /** Result code when attempting to pull multiple into a directory that does not exist. */
    public const int RESULT_NO_DIR_TARGET = 6;
    /** Result code for wrong encoding on the remote path. */
    public const int RESULT_REMOTE_PATH_ENCODING = 7;
    /** Result code for remote path that is too long. */
    public const int RESULT_REMOTE_PATH_LENGTH = 8;
    /** Result code for error while writing local file. */
    public const int RESULT_FILE_WRITE_ERROR = 9;
    /** Result code for error while reading local file. */
    public const int RESULT_FILE_READ_ERROR = 10;
    /** Result code for attempting to push a file that does not exist. */
    public const int RESULT_NO_LOCAL_FILE = 11;
    /** Result code for attempting to push a directory. */
    public const int RESULT_LOCAL_IS_DIRECTORY = 12;
    /** Result code for when the target path of a multi file push is a file. */
    public const int RESULT_REMOTE_IS_FILE = 13;
    /** Result code for receiving too much data from the remove device at once */
    public const int RESULT_BUFFER_OVERRUN = 14;


		public static String ErrorCodeToString ( int code ) {
			switch ( code ) {
				case RESULT_OK:
					return "Success.";
				case RESULT_CANCELED:
					return "Tranfert canceled by the user.";
				case RESULT_UNKNOWN_ERROR:
					return "Unknown Error.";
				case RESULT_CONNECTION_ERROR:
					return "Adb Connection Error.";
				case RESULT_NO_REMOTE_OBJECT:
					return "Remote object doesn't exist!";
				case RESULT_TARGET_IS_FILE:
					return "Target object is a file.";
				case RESULT_NO_DIR_TARGET:
					return "Target directory doesn't exist.";
				case RESULT_REMOTE_PATH_ENCODING:
					return "Remote Path encoding is not supported.";
				case RESULT_REMOTE_PATH_LENGTH:
					return "Remove path is too long.";
				case RESULT_FILE_WRITE_ERROR:
					return "Writing local file failed!";
				case RESULT_FILE_READ_ERROR:
					return "Reading local file failed!";
				case RESULT_NO_LOCAL_FILE:
					return "Local file doesn't exist.";
				case RESULT_LOCAL_IS_DIRECTORY:
					return "Local path is a directory.";
				case RESULT_REMOTE_IS_FILE:
					return "Remote path is a file.";
				case RESULT_BUFFER_OVERRUN:
					return "Receiving too much data.";
				default:
					return "Unknown error code.";
			}

		}
	}
}
