#!/usr/bin/env python3
"""
HTTP Server with Range header support for BITS compatibility.
Usage: python server.py [port] [directory]
"""

import os
import sys
import argparse
from http.server import HTTPServer, SimpleHTTPRequestHandler
from functools import partial


class RangeHTTPRequestHandler(SimpleHTTPRequestHandler):
    """HTTP handler with Range header support for BITS/resume downloads."""

    def send_head(self):
        path = self.translate_path(self.path)
        
        if os.path.isdir(path):
            return super().send_head()
        
        if not os.path.exists(path):
            self.send_error(404, "File not found")
            return None

        file_size = os.path.getsize(path)
        
        # Check for Range header
        range_header = self.headers.get('Range')
        
        if range_header:
            return self.send_partial_content(path, file_size, range_header)
        else:
            return self.send_full_content(path, file_size)

    def send_full_content(self, path, file_size):
        """Send complete file with Accept-Ranges header."""
        try:
            f = open(path, 'rb')
        except OSError:
            self.send_error(404, "File not found")
            return None

        self.send_response(200)
        self.send_header("Content-Type", self.guess_type(path))
        self.send_header("Content-Length", str(file_size))
        self.send_header("Accept-Ranges", "bytes")
        self.end_headers()
        return f

    def send_partial_content(self, path, file_size, range_header):
        """Handle Range request and send partial content (HTTP 206)."""
        try:
            # Parse Range header: "bytes=start-end" or "bytes=start-"
            range_spec = range_header.replace("bytes=", "")
            
            if "-" not in range_spec:
                self.send_error(416, "Invalid Range")
                return None
            
            parts = range_spec.split("-")
            start = int(parts[0]) if parts[0] else 0
            end = int(parts[1]) if parts[1] else file_size - 1
            
            # Validate range
            if start >= file_size or end >= file_size or start > end:
                self.send_error(416, "Range Not Satisfiable")
                return None
            
            content_length = end - start + 1
            
            f = open(path, 'rb')
            f.seek(start)
            
            self.send_response(206)
            self.send_header("Content-Type", self.guess_type(path))
            self.send_header("Content-Length", str(content_length))
            self.send_header("Content-Range", f"bytes {start}-{end}/{file_size}")
            self.send_header("Accept-Ranges", "bytes")
            self.end_headers()
            
            # Return a wrapper that only reads the requested range
            return RangeFile(f, content_length)
            
        except (ValueError, IndexError):
            self.send_error(416, "Invalid Range header")
            return None

    def copyfile(self, source, outputfile):
        """Copy file, respecting range limits if applicable."""
        if isinstance(source, RangeFile):
            remaining = source.remaining
            while remaining > 0:
                chunk_size = min(64 * 1024, remaining)
                chunk = source.file.read(chunk_size)
                if not chunk:
                    break
                outputfile.write(chunk)
                remaining -= len(chunk)
            source.file.close()
        else:
            super().copyfile(source, outputfile)


class RangeFile:
    """Wrapper for file with range limit."""
    def __init__(self, file, remaining):
        self.file = file
        self.remaining = remaining
    
    def read(self, size=-1):
        if size < 0 or size > self.remaining:
            size = self.remaining
        data = self.file.read(size)
        self.remaining -= len(data)
        return data
    
    def close(self):
        self.file.close()


def run_server(port=8080, directory="."):
    os.chdir(directory)
    handler = RangeHTTPRequestHandler
    
    with HTTPServer(("0.0.0.0", port), handler) as httpd:
        print(f"Serving HTTP on 0.0.0.0 port {port}")
        print(f"Directory: {os.path.abspath(directory)}")
        print(f"Range header: Supported (BITS compatible)")
        print(f"URL: http://0.0.0.0:{port}/")
        print("Press Ctrl+C to stop...")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="HTTP Server with Range support")
    parser.add_argument("port", type=int, nargs="?", default=8080, help="Port (default: 8080)")
    parser.add_argument("directory", nargs="?", default=".", help="Directory to serve (default: .)")
    args = parser.parse_args()
    
    run_server(args.port, args.directory)

