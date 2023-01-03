
# Tools
CC=gcc
CFLAGS=-fPIC -fpermissive -Wall

TARGET=libPuttyDLL.so

OBJECTS=callback.o config.o export.o minibidi.o misc.o terminal.o tree234.o version.o wildcard.o

all: $(TARGET)


%.o: %.c
	$(CC) -c $(CFLAGS) $(CPPFLAGS) $< -o $@

export.o: WINDOWS/MSVC/PUTTYDLL/export.c
	$(CC) -c $(CFLAGS) $(CPPFLAGS) $< -o $@

$(TARGET): $(OBJECTS)
	$(CC) -shared -o $@ $^

clean:
	rm -f $(TARGET) *.o
