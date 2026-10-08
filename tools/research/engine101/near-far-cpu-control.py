import json
from unicorn import Uc, UC_ARCH_X86, UC_MODE_16
from unicorn.x86_const import UC_X86_REG_CS, UC_X86_REG_IP, UC_X86_REG_SS, UC_X86_REG_SP, UC_X86_REG_AX
u=Uc(UC_ARCH_X86,UC_MODE_16);u.mem_map(0,0x100000)
u.mem_write(0x10000,bytes.fromhex('e8 01 00 cb 58 0e 50 cb'))
u.mem_write(0x9e000,bytes.fromhex('00 01 00 10'))
u.reg_write(UC_X86_REG_CS,0x1000);u.reg_write(UC_X86_REG_IP,0);u.reg_write(UC_X86_REG_SS,0x9000);u.reg_write(UC_X86_REG_SP,0xe000)
u.emu_start(0x10000,0x10100,count=12)
r={'ax':u.reg_read(UC_X86_REG_AX),'cs':u.reg_read(UC_X86_REG_CS),'ip':u.reg_read(UC_X86_REG_IP),'sp':u.reg_read(UC_X86_REG_SP)}
assert r=={'ax':3,'cs':0x1000,'ip':0x100,'sp':0xe004},r
print(json.dumps(r))
