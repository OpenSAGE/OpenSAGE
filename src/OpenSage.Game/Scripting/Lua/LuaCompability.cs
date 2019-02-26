using MoonSharp.Interpreter;

namespace OpenSage.Scripting.Lua
{
    /// <summary>
    /// Compability code for LUA 4.0.1
    /// </summary>
    public class LuaCompatibility
    {
        private const string _compabilityCode = @"
                globals = _G
                function getn(table) return #table end
                closefile = io.close
                flush = io.flush
                openfile = io.open
                read = io.read
                tmpname = os.tmpname
                write = io.write
                abs = math.abs
                acos = math.acos
                asin = math.asin
                atan = math.atan
                atan2 = math.atan2
                ceil = math.ceil
                cos = math.cos
                cosh = math.cosh
                deg = math.deg
                exp = math.exp
                floor = math.floor
                mod = math.fmod
                mod2 = math.modf
                frexp = math.frexp
                ldexp = math.ldexp
                log = math.log
                max = math.max
                min = math.min
                PI = math.pi
                randomseed = math.randomseed
                rad = math.rad
                random = math.random
                sin = math.sin
                sqrt = math.sqrt
                tan = math.tan
                clock = os.clock
                date = os.date
                execute = os.execute
                exit = os.exit
                getenv = os.getenv
                remove = os.remove
                rename = os.rename
                setlocale = os.setlocale
                strbyte = string.byte
                strchar = string.char
                strfind = string.find
                format = string.format
                gsub = string.gsub
                strlen = string.len
                strlower = string.lower
                strrep = string.rep
                strsub = string.sub
                strupper = string.upper
                tinsert = table.insert
                tremove = table.remove
                sort = table.sort
                function log10(number) return math.log(number,10) end
                call = pcall
                function seek(filehandle, whence, offset) return filehandle:seek(whence, offset) end
                rawgettable = rawget
                rawsettable = rawset
                function getglobal(index) return _G[index] end
                function setglobal(index, value) _G[index]=value end
                function foreach(t, f)
                    for i, v in t do
                        local res = f(i, v)
                        if res then return res end
                    end
                end
                function foreachi(t, f)
                    for i=1,#(t) do
                        local res = f(i, t[i])
                        if res then return res end
                    end
                end
                function readfrom(file) _INPUT = io.open(file,'r') return io.read(file) end
                function writeto(file) _OUTPUT = io.open(file,'w+') return io.output(file) end
                function appendto(file) _OUTPUT = io.open(file,'a+') return io.output(file) end
                --debug = debug.debug
                function rawgetglobal(index) rawget(_G, index) end
                function rawsetglobal(index, value) return rawset(_G, index, value) end
                function _ALERT(...) print('error') end
                function _ERRORMESSAGE(...) print('error') end";
        //unsupported (and never used in any SAGE game and it's mods): debug and tag methods, %upvalues
        //DEPRECATED and actually removed from original SAGE: rawgetglobal, rawsetglobal, foreachvar, nextvar

        public static void Apply(Script script)
        {
            script.DoString(_compabilityCode);
        }
    }
}
