#!/usr/bin/env python3

"""
Documentation and Implementation Validator for VDF Perceptual Hashing
This script validates the implementation against the documented requirements.
"""

import os
import re
import json
from pathlib import Path
from typing import Dict, List, Tuple, Optional

class ImplementationValidator:
    def __init__(self, base_path: str):
        self.base_path = Path(base_path)
        self.results = []
        
    def log(self, level: str, message: str, details: str = ""):
        self.results.append({
            "level": level,
            "message": message,
            "details": details
        })
        
    def check_file_exists(self, filepath: str, description: str) -> bool:
        """Check if a file exists and log the result."""
        full_path = self.base_path / filepath
        exists = full_path.exists()
        
        if exists:
            self.log("SUCCESS", f"{description} exists", str(full_path))
            return True
        else:
            self.log("ERROR", f"{description} missing", str(full_path))
            return False
            
    def check_code_pattern(self, filepath: str, pattern: str, description: str, required: bool = True) -> bool:
        """Check if a code pattern exists in a file."""
        full_path = self.base_path / filepath
        
        if not full_path.exists():
            if required:
                self.log("ERROR", f"Cannot check pattern in missing file: {filepath}")
            return False
            
        try:
            content = full_path.read_text(encoding='utf-8')
            matches = re.findall(pattern, content, re.MULTILINE | re.IGNORECASE)
            
            if matches:
                self.log("SUCCESS", f"{description} found", f"Pattern: {pattern}")
                return True
            else:
                level = "ERROR" if required else "WARNING"
                self.log(level, f"{description} not found", f"Pattern: {pattern}")
                return False
                
        except Exception as e:
            self.log("ERROR", f"Error reading file {filepath}", str(e))
            return False
            
    def analyze_complexity(self, filepath: str) -> Dict[str, int]:
        """Analyze code complexity metrics."""
        full_path = self.base_path / filepath
        
        if not full_path.exists():
            return {}
            
        try:
            content = full_path.read_text(encoding='utf-8')
            
            # Count various code elements
            metrics = {
                "lines": len(content.splitlines()),
                "methods": len(re.findall(r'(public|private|internal|protected).*?\w+\s*\(', content)),
                "classes": len(re.findall(r'(class|struct|interface)\s+\w+', content)),
                "properties": len(re.findall(r'(public|private|internal|protected).*?\w+\s*{\s*(get|set)', content)),
                "try_blocks": len(re.findall(r'try\s*{', content)),
                "catch_blocks": len(re.findall(r'catch\s*\(', content)),
            }
            
            return metrics
            
        except Exception as e:
            self.log("ERROR", f"Error analyzing complexity of {filepath}", str(e))
            return {}
            
    def validate_perceptual_hash_struct(self):
        """Validate PerceptualHash struct implementation."""
        filepath = "VDF.Core/Utils/PerceptualHash.cs"
        
        if not self.check_file_exists(filepath, "PerceptualHash struct"):
            return
            
        # Check required components
        patterns = [
            (r'struct\s+PerceptualHash', "PerceptualHash struct declaration"),
            (r'ulong\s+Hash[1-4]', "Hash components (Hash1-4)"),
            (r'HammingDistance', "Hamming distance method"),
            (r'SimilarityPercentage', "Similarity percentage method"),
            (r'Popcnt\.IsSupported', "Hardware acceleration detection"),
            (r'PopCountSoftware', "Software fallback implementation"),
            (r'ProtoMember', "Protobuf serialization attributes"),
            (r'ToBytes|FromBytes', "Serialization methods"),
        ]
        
        for pattern, description in patterns:
            self.check_code_pattern(filepath, pattern, description)
            
        # Analyze complexity
        metrics = self.analyze_complexity(filepath)
        if metrics:
            self.log("INFO", f"PerceptualHash complexity", 
                    f"Lines: {metrics.get('lines', 0)}, Methods: {metrics.get('methods', 0)}")
                    
    def validate_hash_utils_class(self):
        """Validate HashUtils class implementation."""
        filepath = "VDF.Core/Utils/HashUtils.cs"
        
        if not self.check_file_exists(filepath, "HashUtils class"):
            return
            
        patterns = [
            (r'class\s+HashUtils', "HashUtils class declaration"),
            (r'ComputeImageHash', "Image hash computation method"),
            (r'ComputeVideoHash', "Video hash computation method"),
            (r'ComputeDCT', "DCT computation method"),
            (r'ResizeImage', "Image resizing method"),
            (r'CombineFrameHashes', "Frame combination method"),
            (r'median', "Median calculation"),
        ]
        
        for pattern, description in patterns:
            self.check_code_pattern(filepath, pattern, description)
            
    def validate_file_entry_modifications(self):
        """Validate FileEntry modifications."""
        filepath = "VDF.Core/FileEntry.cs"
        
        if not self.check_file_exists(filepath, "FileEntry class"):
            return
            
        patterns = [
            (r'PerceptualHashBytes.*byte\[\]\?', "PerceptualHashBytes property"),
            (r'ProtoMember\s*\(\s*9\s*\)', "ProtoMember(9) attribute"),
            (r'PerceptualHash\?.*get.*set', "PerceptualHash property accessor"),
        ]
        
        for pattern, description in patterns:
            self.check_code_pattern(filepath, pattern, description)
            
    def validate_settings_modifications(self):
        """Validate Settings class modifications."""
        filepath = "VDF.Core/Settings.cs"
        
        if not self.check_file_exists(filepath, "Settings class"):
            return
            
        patterns = [
            (r'UseFastHashing.*bool', "UseFastHashing property"),
            (r'FastHashingThreshold.*int', "FastHashingThreshold property"),
            (r'FastHashingSimilarityThreshold.*float', "FastHashingSimilarityThreshold property"),
        ]
        
        for pattern, description in patterns:
            self.check_code_pattern(filepath, pattern, description)
            
    def validate_scan_engine_integration(self):
        """Validate ScanEngine integration."""
        filepath = "VDF.Core/ScanEngine.cs"
        
        if not self.check_file_exists(filepath, "ScanEngine class"):
            return
            
        patterns = [
            (r'CheckIfDuplicateWithFastHashing', "Fast hashing duplicate check method"),
            (r'HashUtils\.ComputeImageHash', "Image hash computation integration"),
            (r'HashUtils\.ComputeVideoHash', "Video hash computation integration"),
            (r'Settings\.UseFastHashing', "Fast hashing setting usage"),
            (r'Settings\.FastHashingThreshold', "Threshold setting usage"),
            (r'useFastHashing.*=.*Settings\.UseFastHashing', "Fast hashing logic"),
            (r'entry\.PerceptualHash', "Perceptual hash property access"),
        ]
        
        for pattern, description in patterns:
            self.check_code_pattern(filepath, pattern, description)
            
        # Check for performance optimization logic
        self.check_code_pattern(filepath, r'ScanList\.Count.*>=.*FastHashingThreshold', 
                              "Threshold-based fast hashing activation")
                              
    def validate_gui_integration(self):
        """Validate GUI integration."""
        files_patterns = [
            ("VDF.GUI/Data/SettingsFile.cs", [
                (r'UseFastHashing', "GUI UseFastHashing property"),
                (r'FastHashingThreshold', "GUI FastHashingThreshold property"),
                (r'FastHashingSimilarityThreshold', "GUI FastHashingSimilarityThreshold property"),
                (r'JsonPropertyName', "JSON serialization attributes"),
            ]),
            ("VDF.GUI/ViewModels/MainWindowVM.cs", [
                (r'Scanner\.Settings\.UseFastHashing.*=.*SettingsFile\.UseFastHashing', "Settings transfer logic"),
                (r'Scanner\.Settings\.FastHashingThreshold', "Threshold settings transfer"),
            ]),
            ("VDF.GUI/Views/MainWindow.xaml", [
                (r'Fast.*Hashing.*CheckBox', "Fast hashing UI checkbox"),
                (r'Fast.*Hashing.*Threshold', "Threshold UI controls"),
            ]),
        ]
        
        for filepath, patterns in files_patterns:
            if self.check_file_exists(filepath, f"GUI file: {filepath}"):
                for pattern, description in patterns:
                    self.check_code_pattern(filepath, pattern, description, required=False)
                    
    def check_potential_issues(self):
        """Check for potential implementation issues."""
        
        # Check for old method name usage
        self.check_code_pattern("VDF.Core/Utils/PerceptualHash.cs", 
                              r'GetSimilarityPercentage', 
                              "Old method name usage (should be fixed)", 
                              required=False)
        
        # Check for proper null handling
        files_to_check = [
            "VDF.Core/ScanEngine.cs",
            "VDF.Core/Utils/HashUtils.cs"
        ]
        
        for filepath in files_to_check:
            full_path = self.base_path / filepath
            if full_path.exists():
                content = full_path.read_text(encoding='utf-8')
                null_checks = len(re.findall(r'!= null|== null|\?\?|\?\.', content))
                self.log("INFO", f"Null safety patterns in {filepath}", f"Found {null_checks} patterns")
                
    def generate_report(self) -> str:
        """Generate a comprehensive validation report."""
        report = []
        report.append("=== VDF Perceptual Hashing Implementation Validation ===")
        report.append(f"Base path: {self.base_path}")
        report.append("")
        
        # Count results by level
        counts = {"SUCCESS": 0, "WARNING": 0, "ERROR": 0, "INFO": 0}
        for result in self.results:
            counts[result["level"]] += 1
            
        report.append(f"Summary: {counts['SUCCESS']} successes, {counts['WARNING']} warnings, {counts['ERROR']} errors")
        report.append("")
        
        # Group results by level
        for level in ["ERROR", "WARNING", "SUCCESS", "INFO"]:
            level_results = [r for r in self.results if r["level"] == level]
            if level_results:
                report.append(f"--- {level} ({len(level_results)}) ---")
                for result in level_results:
                    report.append(f"  {result['message']}")
                    if result["details"]:
                        report.append(f"    {result['details']}")
                report.append("")
                
        return "\n".join(report)
        
    def run_validation(self):
        """Run complete validation."""
        self.log("INFO", "Starting validation", f"Base path: {self.base_path}")
        
        # Core implementation validation
        self.validate_perceptual_hash_struct()
        self.validate_hash_utils_class()
        self.validate_file_entry_modifications()
        self.validate_settings_modifications()
        self.validate_scan_engine_integration()
        
        # GUI integration validation
        self.validate_gui_integration()
        
        # Issue detection
        self.check_potential_issues()
        
        return self.generate_report()

def main():
    base_path = "/home/leo/videoduplicatefinder"
    validator = ImplementationValidator(base_path)
    
    report = validator.run_validation()
    print(report)
    
    # Save report to file
    report_file = Path(base_path) / "validation_report.txt"
    report_file.write_text(report)
    print(f"\nValidation report saved to: {report_file}")

if __name__ == "__main__":
    main()
