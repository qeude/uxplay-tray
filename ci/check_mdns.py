"""Fail unless a UxPlay server named argv[1] is advertised via mDNS (_airplay and _raop)."""
import sys, time
from zeroconf import ServiceBrowser, Zeroconf

name = sys.argv[1]
found = {}

class Listener:
    def add_service(self, zc, type_, svc):
        info = zc.get_service_info(type_, svc)
        print("found", type_, svc, info.parsed_addresses() if info else "", info.port if info else "")
        if name in svc:
            found[type_] = info
    update_service = remove_service = lambda *a: None

zc = Zeroconf()
types = ["_airplay._tcp.local.", "_raop._tcp.local."]
browsers = [ServiceBrowser(zc, t, Listener()) for t in types]
deadline = time.time() + 30
while time.time() < deadline and len(found) < len(types):
    time.sleep(0.5)
zc.close()
missing = [t for t in types if t not in found]
if missing:
    sys.exit(f"not advertised: {missing}")
print("OK: UxPlay is discoverable without Bonjour")
