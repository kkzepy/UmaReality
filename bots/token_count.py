import sys
from transformers import AutoTokenizer

def count_hf_tokens(text, model_id="meta-llama/Meta-Llama-3-8B"):
    tokenizer = AutoTokenizer.from_pretrained(model_id)
    tokens = tokenizer.encode(text)
    return len(tokens)

if len(sys.argv) > 1:
    tokens = count_hf_tokens(sys.argv[1], sys.argv[2])
    print(tokens)