# script_cache_ages

Signed age counters for the 16 script-cache slots at `4C13:0239` in
BLD-GOG-EN-1.1. Eligible values zero through 126 are incremented at the
branch-specific points in FND-SCRIPT-019 and RULE-SCRIPT-010. Matching or
newly filled slots are reset to zero; current-pair early reuse does not
age slots. These counters are not an unconditional count of script calls.

Replacement compares ages as signed values, keeping the first largest
age, then sets the selected age to minus one (FND-SCRIPT-021).
