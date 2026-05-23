#!/usr/bin/env python3
"""
C# Code Reorganizer
Splits C# files by namespace and type into separate files.
"""

import os
import re
from pathlib import Path
from typing import List, Dict, Tuple, Optional

# Required using statements for each file
REQUIRED_USINGS = """using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime;
using System.Runtime.Serialization;
"""

# Project header to add at the top of each file
PROJECT_HEADER = """//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//  Project :           Michitai.Lan
//  Author  :           Nichita Levandovici
//  Date    :           22.05.2026
//  Email   :           support@michitai.com
//  Website :           https://michitai.com
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

"""

class CSType:
    def __init__(self, type_kind: str, name: str, content: str, start_line: int, end_line: int):
        self.type_kind = type_kind  # 'class', 'struct', 'enum', 'interface'
        self.name = name
        self.content = content
        self.start_line = start_line
        self.end_line = end_line

class CSNamespace:
    def __init__(self, full_name: str, content: str, start_line: int, end_line: int):
        self.full_name = full_name
        self.content = content
        self.start_line = start_line
        self.end_line = end_line
        self.types: List[CSType] = []

def find_namespace_end(lines: List[str], start_idx: int) -> int:
    """Find the closing brace for a namespace declaration."""
    brace_count = 0
    in_namespace = False
    
    for i in range(start_idx, len(lines)):
        line = lines[i]
        
        # Count braces, ignoring comments and strings
        for char in line:
            if char == '{':
                brace_count += 1
                in_namespace = True
            elif char == '}':
                brace_count -= 1
                if in_namespace and brace_count == 0:
                    return i
    
    return len(lines) - 1

def extract_nested_namespaces(content: str) -> List[CSNamespace]:
    """Extract all nested namespaces with their full paths from C# code."""
    lines = content.split('\n')
    namespaces = []
    
    # Pattern to match namespace declarations (more flexible)
    namespace_pattern = re.compile(r'^\s*namespace\s+([a-zA-Z0-9_.]+)')
    
    # Track current namespace stack
    namespace_stack = []
    
    i = 0
    while i < len(lines):
        match = namespace_pattern.match(lines[i])
        if match:
            namespace_name = match.group(1)
            
            # Build full namespace path
            full_namespace = '.'.join(namespace_stack + [namespace_name])
            
            # Find the end of this namespace
            start_line = i
            end_line = find_namespace_end(lines, i)
            
            # Extract namespace content (including braces)
            namespace_content = '\n'.join(lines[start_line:end_line + 1])
            
            namespace = CSNamespace(full_namespace, namespace_content, start_line, end_line)
            namespaces.append(namespace)
            
            # Push to stack and continue searching within
            namespace_stack.append(namespace_name)
            i += 1
            
            # Check if this namespace has nested namespaces
            has_nested = False
            for j in range(i, end_line):
                if namespace_pattern.match(lines[j]):
                    has_nested = True
                    break
            
            # If no nested namespaces, pop the stack and move to end
            if not has_nested:
                namespace_stack.pop()
                i = end_line + 1
        else:
            # Check if we're exiting a namespace (closing brace)
            line_stripped = lines[i].strip()
            if line_stripped == '}' and namespace_stack:
                namespace_stack.pop()
            i += 1
    
    return namespaces

def find_type_end(lines: List[str], start_idx: int) -> int:
    """Find the closing brace for a type declaration."""
    brace_count = 0
    in_type = False
    
    for i in range(start_idx, len(lines)):
        line = lines[i]
        
        # Count braces, ignoring comments and strings
        for char in line:
            if char == '{':
                brace_count += 1
                in_type = True
            elif char == '}':
                brace_count -= 1
                if in_type and brace_count == 0:
                    return i
    
    return len(lines) - 1

def extract_types(content: str, namespace_start_line: int = 0) -> List[CSType]:
    """Extract all types (class, struct, enum, interface) from C# code."""
    lines = content.split('\n')
    types = []
    
    # Pattern to match type declarations
    # Matches: [attributes] public/private/internal/protected/sealed/abstract/static class/struct/enum/interface Name
    type_pattern = re.compile(
        r'^\s*(?:\[[^\]]+\]\s*)*' +  # Match attributes like [Flags], [Serializable], etc.
        r'(?:public|private|internal|protected|sealed|abstract|static|\s)*' +
        r'(class|struct|enum|interface)\s+([a-zA-Z0-9_<>]+)'
    )
    
    i = 0
    while i < len(lines):
        match = type_pattern.match(lines[i])
        if match:
            type_kind = match.group(1)
            type_name = match.group(2).split('<')[0]  # Remove generic parameters
            start_line = i
            end_line = find_type_end(lines, i)
            
            # Extract type content (including braces)
            type_content = '\n'.join(lines[start_line:end_line + 1])
            
            type_obj = CSType(type_kind, type_name, type_content, 
                            namespace_start_line + start_line, 
                            namespace_start_line + end_line)
            types.append(type_obj)
            
            i = end_line + 1
        else:
            i += 1
    
    return types

def extract_other_usings(content: str) -> List[str]:
    """Extract non-standard using statements from the file."""
    usings = []
    lines = content.split('\n')
    
    for line in lines:
        stripped = line.strip()
        if stripped.startswith('using ') and ';' in stripped:
            using_stmt = stripped
            # Check if it's not one of the standard usings
            standard_usings = [
                'using System;',
                'using System.IO;',
                'using System.Linq;',
                'using System.Collections;',
                'using System.Collections.Generic;',
                'using System.Threading;',
                'using System.Threading.Tasks;',
                'using System.Net;',
                'using System.Net.Sockets;',
                'using System.Net.Security;',
                'using System.Net.NetworkInformation;',
                'using System.Text;',
                'using System.Runtime;',
                'using System.Runtime.Serialization;'
            ]
            if using_stmt not in standard_usings:
                usings.append(using_stmt)
        elif stripped and not stripped.startswith('using ') and not stripped.startswith('//'):
            # Stop at first non-using, non-comment line
            break
    
    return usings

def get_leading_whitespace(content: str) -> str:
    """Get the leading whitespace of the content."""
    lines = content.split('\n')
    if not lines:
        return ''
    
    # Find minimum indentation
    min_indent = float('inf')
    for line in lines[1:]:  # Skip first line (type declaration)
        if line.strip():
            indent = len(line) - len(line.lstrip())
            min_indent = min(min_indent, indent)
    
    if min_indent == float('inf'):
        return ''
    
    return ' ' * min_indent

def normalize_indentation(content: str) -> str:
    """Remove leading indentation from type content."""
    lines = content.split('\n')
    if not lines:
        return content
    
    # Get the leading whitespace
    leading_ws = get_leading_whitespace(content)
    if not leading_ws:
        return content
    
    # Remove the leading whitespace from each line
    result_lines = [lines[0]]  # Keep first line as-is
    for line in lines[1:]:
        if line.startswith(leading_ws):
            result_lines.append(line[len(leading_ws):])
        else:
            result_lines.append(line)
    
    return '\n'.join(result_lines)

def namespace_to_path(namespace: str) -> Path:
    """Convert namespace to folder path."""
    # Replace dots with path separators
    parts = namespace.split('.')
    return Path(*parts)

def create_type_file(output_dir: Path, namespace: CSNamespace, type_obj: CSType, 
                     other_usings: List[str]) -> Path:
    """Create a .cs file for a type."""
    # Create namespace folder structure
    namespace_path = output_dir / namespace_to_path(namespace.full_name)
    namespace_path.mkdir(parents=True, exist_ok=True)
    
    # Create file path
    file_path = namespace_path / f"{type_obj.name}.cs"
    
    # Normalize indentation
    normalized_content = normalize_indentation(type_obj.content)
    
    # Build file content
    file_content = PROJECT_HEADER
    file_content += REQUIRED_USINGS + '\n'
    
    # Add other usings
    if other_usings:
        file_content += '\n'.join(other_usings) + '\n'
    
    file_content += '\n'
    file_content += f"namespace {namespace.full_name}\n"
    file_content += "{\n"
    file_content += normalized_content + '\n'
    file_content += "}\n"
    
    # Write file
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(file_content)
    
    return file_path

def process_cs_file(input_file: Path, output_dir: Path) -> int:
    """Process a single C# file and split it into multiple files."""
    print(f"Processing: {input_file}")
    
    # Read file content
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract other using statements (non-standard)
    other_usings = extract_other_usings(content)
    
    # Extract namespaces
    namespaces = extract_nested_namespaces(content)
    
    if not namespaces:
        print(f"  No namespaces found in {input_file}")
        return 0
    
    files_created = 0
    
    # Extract all types from all namespaces and track their line numbers
    all_types = []
    
    for namespace in namespaces:
        namespace_lines = namespace.content.split('\n')
        if namespace_lines and namespace_lines[0].strip().startswith('namespace'):
            first_line_idx = 0
            for idx, line in enumerate(namespace_lines):
                if '{' in line:
                    first_line_idx = idx
                    break
            
            inner_content = '\n'.join(namespace_lines[first_line_idx + 1:-1])
            types = extract_types(inner_content, namespace.start_line)
            
            for type_obj in types:
                # Calculate actual line number in original file
                actual_line = namespace.start_line + type_obj.start_line
                all_types.append((actual_line, namespace, type_obj))
    
    # Sort by line number and remove duplicates (keep the one in the deepest namespace)
    all_types.sort(key=lambda x: x[0])
    seen_types = {}
    
    for line_num, namespace, type_obj in all_types:
        if type_obj.name not in seen_types:
            seen_types[type_obj.name] = (namespace, type_obj)
        else:
            # Keep the one in the deeper namespace
            existing_ns, _ = seen_types[type_obj.name]
            if len(namespace.full_name.split('.')) > len(existing_ns.full_name.split('.')):
                seen_types[type_obj.name] = (namespace, type_obj)
    
    # Create files for unique types
    for type_name, (namespace, type_obj) in seen_types.items():
        print(f"  Found namespace: {namespace.full_name}")
        print(f"    Type: {type_obj.name}")
        
        file_path = create_type_file(output_dir, namespace, type_obj, other_usings)
        print(f"      Created: {file_path}")
        files_created += 1
    
    return files_created

def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description='Reorganize C# code by splitting types into separate files by namespace.'
    )
    parser.add_argument(
        'input',
        help='Input C# file or directory containing C# files'
    )
    parser.add_argument(
        'output',
        help='Output directory for reorganized files'
    )
    parser.add_argument(
        '--pattern',
        default='*.cs',
        help='File pattern to match (default: *.cs)'
    )
    
    args = parser.parse_args()
    
    input_path = Path(args.input)
    output_dir = Path(args.output)
    
    if not input_path.exists():
        print(f"Error: Input path does not exist: {input_path}")
        return 1
    
    # Create output directory
    output_dir.mkdir(parents=True, exist_ok=True)
    
    total_files = 0
    total_types = 0
    
    if input_path.is_file():
        # Process single file
        if input_path.suffix.lower() != '.cs':
            print(f"Error: Input file must be a .cs file: {input_path}")
            return 1
        
        types_count = process_cs_file(input_path, output_dir)
        total_files += 1
        total_types += types_count
    else:
        # Process directory
        for cs_file in input_path.rglob(args.pattern):
            if cs_file.is_file():
                types_count = process_cs_file(cs_file, output_dir)
                total_files += 1
                total_types += types_count
    
    print(f"\nSummary:")
    print(f"  Files processed: {total_files}")
    print(f"  Type files created: {total_types}")
    print(f"  Output directory: {output_dir}")
    
    return 0

if __name__ == '__main__':
    exit(main())
