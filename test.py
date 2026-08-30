import os
import subprocess

__TEST_CODE_PATH__ = "TestCode/"
__TEST_RUNNER__ = "dotnet run --project Aurora/Aurora.csproj -- {}"
__GREEN_COLOR__ = "\033[32m"
__YELLOW_COLOR__ = "\033[33m"
__RED_COLOR__ = "\033[31m"
__RESET_COLOR__ = "\033[0m"

def test_file(file: str) -> bool:
    print(f"{__RESET_COLOR__}Test {file}: ", end="", flush=True)

    command_to_run = __TEST_RUNNER__.format(os.path.join(__TEST_CODE_PATH__, file))
    result = subprocess.run(command_to_run.split(" "), capture_output=True, text=True)
    return_code = result.returncode
    stdout = result.stdout
    stderr = result.stderr

    if return_code == 0:
        print(f"{__GREEN_COLOR__}Passed")
        return True

    print(f"{__RED_COLOR__}Failed")
    print(stderr)
         

def main():
    files = os.listdir(__TEST_CODE_PATH__)
    succeeded = 0
    failed = 0

    for file in files:
        test_succeeded = test_file(file)
        if test_succeeded:
            succeeded += 1
        if not test_succeeded:
            failed += 1

    succeeded_message = "" if succeeded == 0 else f"{__GREEN_COLOR__}{succeeded} tests passed{__RESET_COLOR__}"
    failed_message = "" if failed == 0 else f"{__RED_COLOR__}{failed} tests failed"
    
    print()
    if (succeeded_message and failed_message):
        print(succeeded_message + " | " + failed_message)
    else:
        print(succeeded_message + failed_message)
    
    print(__RESET_COLOR__)

if __name__ == "__main__":
    main()
