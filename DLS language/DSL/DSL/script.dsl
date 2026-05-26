project {
    filter {
    }

    add file{
        folder src{
                a.txt
                b.txt
                c.txt
        }
    }

    rename file{
        in folder{
            src
        }
        from a.txt to a1.txt
    }
}
